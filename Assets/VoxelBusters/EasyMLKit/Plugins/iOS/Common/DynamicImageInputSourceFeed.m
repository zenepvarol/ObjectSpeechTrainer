//
//  ARKitCameraInputSourceFeed.m
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import "DynamicImageInputSourceFeed.h"
#import <MLImage/GMLImage.h>
#import <MLKitVision/GMLImage+MLKitCompatible.h>
#import <Metal/MTLTexture.h>
#import <Metal/MTLDevice.h>

@interface DynamicImageInputSourceFeed ()
@property(atomic) BOOL started;
@property(nonatomic) BOOL isProcessing;
@property (nonatomic) id<MTLTexture> texture;
@property (nonatomic) id<MTLDevice> device;
@property (nonatomic) id<MTLBuffer> metalBuffer;
@property (nonatomic, assign) CVPixelBufferRef pixelBuffer;
@property (nonatomic) CGRect selectedRegion;
@end


@implementation DynamicImageInputSourceFeed
@synthesize delegate;
@synthesize started;
@synthesize isProcessing = _isProcessing;
@synthesize texture = _texture;
@synthesize device = _device;
@synthesize pixelBuffer = _pixelBuffer;
@synthesize selectedRegion = _selectedRegion;

-(id)initWithTexture:(id<MTLTexture>)texture withSelectedRegion:(CGRect) selectedRegion
{
    self = [super init];
    self.texture = texture;
    self.selectedRegion = selectedRegion;
    [self setup];
    return self;
}

- (void)start {
    self.started = true;
}

- (void)stop {
    self.started = false;
}

- (void)setIsProcessing:(BOOL)status {
    _isProcessing = status;
}

- (void) onCameraFrameReceived
{
    if(started && !_isProcessing)
    {
        if(delegate != NULL)
        {
            [self setIsProcessing: TRUE];

            dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
                [self copyDataFromMTLTexture: self.texture];
                GMLImage* image = [[GMLImage alloc] initWithPixelBuffer: self.pixelBuffer];
                image.orientation = [self getCurrentOrientation];
                [self.delegate didReceiveInputImage:image];
            });
        }
    }
}

- (void)setup {
    
    self.device = MTLCreateSystemDefaultDevice();
    
    long width = self.texture.width;
    long height = self.texture.height;

    // Create MTL buffer
    NSUInteger bytesPerRow = width * 4; // kCVPixelFormatType_32BGRA format
    NSUInteger bufferSize = bytesPerRow * height;
    self.metalBuffer = [self.device newBufferWithLength:bufferSize options:MTLResourceStorageModeShared];

    // Create CVPixel buffer
    NSDictionary *options = @{
        (__bridge NSString *)kCVPixelBufferCGImageCompatibilityKey: @YES,
        (__bridge NSString *)kCVPixelBufferCGBitmapContextCompatibilityKey: @YES
    };
    
    CVReturn status = CVPixelBufferCreate(kCFAllocatorDefault, width, height, kCVPixelFormatType_32BGRA, (__bridge CFDictionaryRef)options, &_pixelBuffer);

    if (status != kCVReturnSuccess) {
        NSLog(@"Error creating CVPixelBuffer");
    }
}

- (void)copyDataFromMTLTexture:(id<MTLTexture>)texture {
    // Lock the base address of the CVPixelBuffer
    CVPixelBufferLockBaseAddress(self.pixelBuffer, 0);

    // Get the contents of the MTLTexture into the Metal buffer
    MTLRegion region = MTLRegionMake2D(0, 0, texture.width, texture.height);
    
    [texture getBytes:self.metalBuffer.contents bytesPerRow:region.size.width * 4 fromRegion:region mipmapLevel:0];

    // Get the base address and bytes per row of the CVPixelBuffer
    void *baseAddress = CVPixelBufferGetBaseAddress(self.pixelBuffer);
    size_t bufferBytesPerRow = CVPixelBufferGetBytesPerRow(self.pixelBuffer);

    // Copy the data from the Metal buffer to the CVPixelBuffer
    for (NSUInteger y = 0; y < region.size.height; y++) {
        size_t sourceOffset = y * region.size.width * 4;
        size_t destinationOffset = y * bufferBytesPerRow;
        memcpy(baseAddress + destinationOffset, self.metalBuffer.contents + sourceOffset, region.size.width * 4);
    }

    // Unlock the base address of the CVPixelBuffer
    CVPixelBufferUnlockBaseAddress(self.pixelBuffer, 0);
}


-(UIImageOrientation) getCurrentOrientation
{
    UIDeviceOrientation deviceOrientation = [[UIDevice currentDevice] orientation];
    switch (deviceOrientation) {
        case UIDeviceOrientationPortrait:
            return UIImageOrientationRight;
        case UIDeviceOrientationLandscapeRight:
            return UIImageOrientationDown;
        default:
            return UIImageOrientationUp;
    }
}

- (float)getInputSourceRotation {
    UIDeviceOrientation deviceOrientation = [[UIDevice currentDevice] orientation];
    switch (deviceOrientation) {
        case UIDeviceOrientationPortrait:
            return 90;
        case UIDeviceOrientationLandscapeRight:
            return 0;
        case UIDeviceOrientationLandscapeLeft:
            return 180;
        default:
            return 0;
    }
}


- (CGSize)getInputSourceSize {
    CGSize size;
    size.width = CVPixelBufferGetWidth(self.pixelBuffer);
    size.height = CVPixelBufferGetHeight(self.pixelBuffer);
    return size;
}

- (void)dealloc
{
    [self.metalBuffer setPurgeableState:MTLPurgeableStateEmpty];
    self.metalBuffer = nil;
    CVPixelBufferRelease(self.pixelBuffer);
    self.pixelBuffer = nil;
}
@end

