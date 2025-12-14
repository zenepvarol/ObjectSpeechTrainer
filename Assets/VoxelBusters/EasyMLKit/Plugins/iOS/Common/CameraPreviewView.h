//
//  ImageInputSourceFeed.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import <Foundation/Foundation.h>
#import "IVisionInputSourceFeed.h"
#import <AVFoundation/AVFoundation.h>

NS_ASSUME_NONNULL_BEGIN
@protocol CameraPreviewViewDelegate;

@interface CameraPreviewView : UIView
-(id) initWithPreviewLayer:(AVCaptureVideoPreviewLayer*) previewLayer withViewport:(CGRect) viewport;
-(void) attachPreviewLayer;

@property (nonatomic)   id<CameraPreviewViewDelegate> delegate;

@end

@protocol CameraPreviewViewDelegate <NSObject>

- (void)didRequestDismiss:(CameraPreviewView *)cameraPreviewView;

@end

NS_ASSUME_NONNULL_END
