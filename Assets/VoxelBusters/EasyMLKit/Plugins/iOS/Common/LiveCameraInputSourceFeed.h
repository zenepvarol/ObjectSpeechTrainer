//
//  ImageInputSourceFeed.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import <Foundation/Foundation.h>
#import "IVisionInputSourceFeed.h"
#import <AVFoundation/AVFoundation.h>
#import "CameraPreviewView.h"


NS_ASSUME_NONNULL_BEGIN

@interface LiveCameraInputSourceFeed : NSObject<IVisionInputSourceFeed, AVCaptureVideoDataOutputSampleBufferDelegate, CameraPreviewViewDelegate>
-(id) initWithOptions:(BOOL) isFrontFacingMode withFlashLight:(BOOL) enableFlashLight withMaxResolution:(CGSize) maxResolution withViewport:(CGRect) viewport;
+ (UIImageOrientation)imageOrientationFromDevicePosition:(AVCaptureDevicePosition)devicePosition;
@end

NS_ASSUME_NONNULL_END
