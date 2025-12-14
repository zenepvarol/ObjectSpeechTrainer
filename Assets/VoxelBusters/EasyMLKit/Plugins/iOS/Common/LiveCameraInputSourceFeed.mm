//
//  ImageInputSourceFeed.m
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import "LiveCameraInputSourceFeed.h"
#import <MLKitVision/MLKVisionImage.h>
#import <MLImage/GMLImage.h>
#import <MLKitVision/GMLImage+MLKitCompatible.h>
#import <UIKit/UIKit.h>
#import "MLKitCommon.h"


@interface LiveCameraInputSourceFeed()
@property (nonatomic)   BOOL isProcessing;
@property (nonatomic)   BOOL isFrontFacingMode;
@property (nonatomic)   BOOL flashLightEnabled;
@property (nonatomic)   CGRect viewport;
@property (nonatomic)   CGSize maxResolution;
@property (nonatomic)   AVCaptureSession *captureSession;
@property (nonatomic)   CameraPreviewView *cameraPreview;
+ (UIImageOrientation)imageOrientationFromDevicePosition:(AVCaptureDevicePosition)devicePosition;

@end

@implementation LiveCameraInputSourceFeed 
@synthesize delegate;
@synthesize isProcessing = _isProcessing;
@synthesize captureSession = _captureSession;
@synthesize cameraPreview = _cameraPreview;
@synthesize isFrontFacingMode = _isFrontFacingMode;
@synthesize flashLightEnabled = _flashLightEnabled;
@synthesize viewport = _viewport;
@synthesize maxResolution = _maxResolution;

-(id) initWithOptions:(BOOL) isFrontFacingMode withFlashLight:(BOOL) enableFlashLight withMaxResolution:(CGSize) maxResolution withViewport:(CGRect) viewport
{
    self = [super init];
    self.isFrontFacingMode = isFrontFacingMode;
    self.flashLightEnabled = enableFlashLight;
    self.viewport = viewport;
    self.maxResolution = maxResolution;
    return self;
}

- (void)start {
    
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        
        self.captureSession = [self createCaptureSession];
        AVCaptureDevice *device = [self getCaptureDevice:self.isFrontFacingMode withFlashLight:self.flashLightEnabled];
        AVCaptureDeviceInput *input = [self createCaptureDeviceInput:device];
        AVCaptureVideoDataOutput *output = [self createCaptureVideoDataOutput];
        
        [self addInputToCaptureSession: input];
        [self addOutputToCaptureSession: output];
        [self attachPreviewLayer];
        [self.captureSession startRunning];
    });
}


- (void)stop {
    [self cleanup];
}

- (void)setIsProcessing:(BOOL)status {
    _isProcessing = status;
}

- (float)getInputSourceRotation {
    return 0;
}

- (CGSize)getInputSourceSize {
    //return inputImage.size;
    return CGSizeZero;
}




-(AVCaptureSession*) createCaptureSession
{
    AVCaptureSession *session = [[AVCaptureSession alloc] init];
    session.sessionPreset = [self getSessionPreset];
    NSLog(@"Selected preset %@", session.sessionPreset);
    return session;
}

-(AVCaptureDevice*) getCaptureDevice :(BOOL) needsFrontFacingCam withFlashLight:(BOOL) isFlashEnabled
{
    AVCaptureDevicePosition camStyle = needsFrontFacingCam ? AVCaptureDevicePositionFront : AVCaptureDevicePositionBack;
    NSArray<AVCaptureDeviceType> *devices;
    
    if (@available(iOS 13.0, *)) {
        devices = @[AVCaptureDeviceTypeBuiltInUltraWideCamera, AVCaptureDeviceTypeBuiltInTripleCamera, AVCaptureDeviceTypeBuiltInWideAngleCamera, AVCaptureDeviceTypeBuiltInDualCamera, AVCaptureDeviceTypeBuiltInWideAngleCamera];
        
    } else {
        // Fallback on earlier versions
        devices = @[AVCaptureDeviceTypeBuiltInWideAngleCamera, AVCaptureDeviceTypeBuiltInDualCamera, AVCaptureDeviceTypeBuiltInWideAngleCamera];
    }
    
    AVCaptureDeviceDiscoverySession *discoverySession = [AVCaptureDeviceDiscoverySession
                                                         discoverySessionWithDeviceTypes:devices
                                                         mediaType:AVMediaTypeVideo
                                                         position:AVCaptureDevicePositionUnspecified];
    
    for (AVCaptureDevice *device in discoverySession.devices) {
      if (device.position == camStyle) {
          NSError *error;
          [device lockForConfiguration:&error];
          if(error == nil)
          {
              if ([device hasTorch]) {
                  device.torchMode = isFlashEnabled ? AVCaptureTorchModeOn : AVCaptureTorchModeOff;
              }
              
              /*if ([device isFocusPointOfInterestSupported])
              {
                  [device setFocusPointOfInterest:CGPointMake(0.5f,0.5f)];
              }*/
              
              if([device isFocusModeSupported:AVCaptureFocusModeContinuousAutoFocus])
              {
                  device.focusMode = AVCaptureFocusModeContinuousAutoFocus;
              }
              else if([device isFocusModeSupported:AVCaptureFocusModeAutoFocus])
              {
                  device.focusMode = AVCaptureFocusModeAutoFocus;
              }
          
              NSLog(@"Focus Mode : %ld", (long)device.focusMode);
          }
          else
          {
              NSLog(@"Failed acquiring lock for changing flash mode configuration");
          }
          [device unlockForConfiguration];
          return device;
      }
    }
    
    return nil;
}

-(AVCaptureDeviceInput*) createCaptureDeviceInput :(AVCaptureDevice*) device
{
    NSError *error = nil;
    AVCaptureDeviceInput *deviceInput = [AVCaptureDeviceInput deviceInputWithDevice:device error:&error];

    if(error == nil)
    {
        return deviceInput;
    }
    else
    {
        NSLog(@"Failed creating capture device input: %@", error.localizedDescription);
        return nil;
    }
}

-(AVCaptureVideoDataOutput*) createCaptureVideoDataOutput
{
    AVCaptureVideoDataOutput *output = [[AVCaptureVideoDataOutput alloc] init];
    [output setVideoSettings:@{
                                (NSString *)kCVPixelBufferPixelFormatTypeKey : [NSNumber numberWithUnsignedInt:kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange]
                                }];
    [output setAlwaysDiscardsLateVideoFrames:YES];

    dispatch_queue_t videoFramesQueue = dispatch_queue_create("VideoFramesQueue", DISPATCH_QUEUE_SERIAL);
    [output setSampleBufferDelegate:self queue:videoFramesQueue];

    return output;
}

- (void)captureOutput:(AVCaptureOutput *)output didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection *)connection {
    MLKVisionImage *visionImage = [[MLKVisionImage alloc] initWithBuffer:sampleBuffer];
    visionImage.orientation = getImageOrientationFromDevicePosition(self.isFrontFacingMode ? AVCaptureDevicePositionFront : AVCaptureDevicePositionBack);//[LiveCameraInputSourceFeed imageOrientationFromDevicePosition: self.isFrontFacingMode ? AVCaptureDevicePositionFront : AVCaptureDevicePositionBack];
    [self.delegate didReceiveInputImage:visionImage];
}


-(void) addInputToCaptureSession:(AVCaptureDeviceInput*) input
{
    if([self.captureSession canAddInput:input])
    {
        [self.captureSession addInput:input];
    }
    else
    {
        NSLog(@"Failed adding capture device input to session!");
    }
}

-(void) addOutputToCaptureSession:(AVCaptureVideoDataOutput*) output
{
    if([self.captureSession canAddOutput:output])
    {
        [self.captureSession addOutput: output];
    }
    else
    {
        NSLog(@"Failed adding capture device input to session!");
    }
}

-(void) attachPreviewLayer
{
    dispatch_sync(dispatch_get_main_queue(), ^{
        
        AVCaptureVideoPreviewLayer* preview = [AVCaptureVideoPreviewLayer layerWithSession:self.captureSession];
        preview.videoGravity = AVLayerVideoGravityResizeAspectFill;
        
        UIView* rootView = UnityGetGLViewController().view;
        self.cameraPreview = [[CameraPreviewView alloc] initWithPreviewLayer:preview withViewport:self.viewport];
        self.cameraPreview.delegate = self;
        [rootView addSubview:self.cameraPreview];
        
        [self.cameraPreview attachPreviewLayer];
    });
}



-(void) cleanup
{
    if([self.captureSession isRunning])
    {
        [self.captureSession stopRunning];
    }
    
    [self detachPreviewLayer];
}

-(void) detachPreviewLayer
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [self.cameraPreview removeFromSuperview];
        self.cameraPreview = nil;
    });
}

- (void)didRequestDismiss:(nonnull CameraPreviewView *)cameraPreviewView { 
    [self.delegate didRequestStop:self];
}

- (AVCaptureSessionPreset) getSessionPreset
{
    if(self.maxResolution.width >= 3840 && [self.captureSession canSetSessionPreset:AVCaptureSessionPreset3840x2160])
    {
        return AVCaptureSessionPreset3840x2160;
    }
    else if(self.maxResolution.width >= 1920 && [self.captureSession canSetSessionPreset:AVCaptureSessionPreset1920x1080])
    {
        return AVCaptureSessionPreset1920x1080;
    }
    else if(self.maxResolution.width >= 1280 && [self.captureSession canSetSessionPreset:AVCaptureSessionPreset1280x720])
    {
        return AVCaptureSessionPreset1280x720;
    }
    else if(self.maxResolution.width >= 960 )
    {
        return AVCaptureSessionPresetHigh;
    }
    else if(self.maxResolution.width >= 400)
    {
        return AVCaptureSessionPresetMedium;
    }
    else if(self.maxResolution.width >= 100)
    {
        return AVCaptureSessionPresetLow;
    }
    else
    {
        return AVCaptureSessionPresetHigh;
    }
}

@end

