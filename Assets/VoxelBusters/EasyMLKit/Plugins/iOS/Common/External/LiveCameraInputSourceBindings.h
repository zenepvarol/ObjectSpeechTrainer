//
//  ImageInputSourceFeedBindings.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 03/06/22.
//

#import <Foundation/Foundation.h>
#import "NPKit.h"

NS_ASSUME_NONNULL_BEGIN

NPBINDING DONTSTRIP void* MLKit_LiveCameraInputSourceFeed_Init(BOOL isFrontFacingMode, BOOL enableFlashLight, NPSize maxResolution,NPUnityRect viewport);

NS_ASSUME_NONNULL_END
