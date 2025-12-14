//
//  MLKitCommon.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import "MLKitCommon.h"

MLKitRect getRect(CGRect source)
{
    MLKitRect rect;
    rect.x = source.origin.x;
    rect.y = source.origin.y;
    rect.width = source.size.width;
    rect.height = source.size.height;
    return rect;
}



UIImageOrientation getImageOrientationFromDevicePosition(AVCaptureDevicePosition devicePosition)
{
  UIDeviceOrientation deviceOrientation = UIDevice.currentDevice.orientation;
  if (deviceOrientation == UIDeviceOrientationFaceDown ||
      deviceOrientation == UIDeviceOrientationFaceUp ||
      deviceOrientation == UIDeviceOrientationUnknown) {
      deviceOrientation = getCurrentUIOrientation();
  }
  switch (deviceOrientation) {
    case UIDeviceOrientationPortrait:
      return devicePosition == AVCaptureDevicePositionFront ? UIImageOrientationLeftMirrored
                                                            : UIImageOrientationRight;
    case UIDeviceOrientationLandscapeLeft:
      return devicePosition == AVCaptureDevicePositionFront ? UIImageOrientationDownMirrored
                                                            : UIImageOrientationUp;
    case UIDeviceOrientationPortraitUpsideDown:
      return devicePosition == AVCaptureDevicePositionFront ? UIImageOrientationRightMirrored
                                                            : UIImageOrientationLeft;
    case UIDeviceOrientationLandscapeRight:
      return devicePosition == AVCaptureDevicePositionFront ? UIImageOrientationUpMirrored
                                                            : UIImageOrientationDown;
    case UIDeviceOrientationFaceDown:
    case UIDeviceOrientationFaceUp:
    case UIDeviceOrientationUnknown:
      return UIImageOrientationUp;
  }
}

UIDeviceOrientation getCurrentUIOrientation()
{
  UIDeviceOrientation (^deviceOrientation)(void) = ^UIDeviceOrientation(void) {
    switch (UIApplication.sharedApplication.statusBarOrientation) {
      case UIInterfaceOrientationLandscapeLeft:
        return UIDeviceOrientationLandscapeRight;
      case UIInterfaceOrientationLandscapeRight:
        return UIDeviceOrientationLandscapeLeft;
      case UIInterfaceOrientationPortraitUpsideDown:
        return UIDeviceOrientationPortraitUpsideDown;
      case UIInterfaceOrientationPortrait:
      case UIInterfaceOrientationUnknown:
        return UIDeviceOrientationPortrait;
    }
  };

  if (NSThread.isMainThread) {
    return deviceOrientation();
  } else {
    __block UIDeviceOrientation currentOrientation = UIDeviceOrientationPortrait;
    dispatch_sync(dispatch_get_main_queue(), ^{
      currentOrientation = deviceOrientation();
    });
    return currentOrientation;
  }
}
