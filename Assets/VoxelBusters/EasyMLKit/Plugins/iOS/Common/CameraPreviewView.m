//
//  ImageInputSourceFeed.m
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import "CameraPreviewView.h"
#import <UIKit/UIKit.h>

@interface CameraPreviewView()
@property (nonatomic)   AVCaptureVideoPreviewLayer *previewLayer;
@property (nonatomic)   CGRect viewport;
@end

@implementation CameraPreviewView
@synthesize previewLayer = _preview;
@synthesize delegate = _delegate;
@synthesize viewport = _viewport;

-(id) initWithPreviewLayer:(AVCaptureVideoPreviewLayer*) previewLayer withViewport:(CGRect) viewport
{
    self = [super init];
    self.previewLayer = previewLayer;
    self.viewport = viewport;
    return self;
}

- (void)layoutSubviews {
    [super layoutSubviews];
    
    self.frame = [self getFrameFromViewport];
    
    // Update the frame of the preview layer to match the custom view's size
    self.previewLayer.frame = CGRectMake(0, 0, self.frame.size.width, self.frame.size.height);
}


-(void) attachPreviewLayer
{
    self.previewLayer.videoGravity = AVLayerVideoGravityResizeAspectFill;
    self.previewLayer.connection.videoOrientation = [self toVideoOrientation];
    [self.layer addSublayer:self.previewLayer];
    
    
    /*UIButton *closeButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [closeButton setTitle:@"Done" forState:UIControlStateNormal];

    [closeButton addTarget:self action:@selector(requestDismiss) forControlEvents:UIControlEventTouchUpInside];
    [closeButton setTranslatesAutoresizingMaskIntoConstraints: NO];
    closeButton.frame = CGRectMake(20, 50, 80, 80); // Adjust the frame as needed

    // Add close button to the window
    [self addSubview:closeButton];
    
    
    [NSLayoutConstraint activateConstraints:@[
        [closeButton.topAnchor constraintEqualToAnchor:self.topAnchor constant:20],
        [closeButton.leadingAnchor constraintEqualToAnchor:self.leadingAnchor constant:20],
        [closeButton.widthAnchor constraintEqualToConstant:80],
        [closeButton.heightAnchor constraintEqualToConstant:80]
    ]];
    */
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(didOrientationChange)
                                                 name:UIDeviceOrientationDidChangeNotification
                                               object:nil];
}


-(void) didOrientationChange
{
    self.previewLayer.connection.videoOrientation = [self toVideoOrientation];
}


- (AVCaptureVideoOrientation) toVideoOrientation {
    
    UIInterfaceOrientation orientation =  [UIApplication sharedApplication].statusBarOrientation;
      switch (orientation) {
        case UIInterfaceOrientationPortrait:
            return AVCaptureVideoOrientationPortrait;
        case UIInterfaceOrientationPortraitUpsideDown:
            return AVCaptureVideoOrientationPortraitUpsideDown;
        case UIInterfaceOrientationLandscapeLeft:
            return AVCaptureVideoOrientationLandscapeLeft;
        case UIInterfaceOrientationLandscapeRight:
            return AVCaptureVideoOrientationLandscapeRight;
        default:
            break;
      }
    
    return AVCaptureVideoOrientationPortrait;
}

-(void) requestDismiss
{
    [self.delegate didRequestDismiss:self];
}

-(CGRect) getFrameFromViewport
{
    
    CGRect rootFrame = self.superview.frame;
    CGRect viewFrame = CGRectMake(self.viewport.origin.x * rootFrame.size.width + rootFrame.origin.x,
                                       self.viewport.origin.y * rootFrame.size.height + rootFrame.origin.y,
                                       self.viewport.size.width * rootFrame.size.width,
                                       self.viewport.size.height * rootFrame.size.height);
    
    return viewFrame;
}

@end

