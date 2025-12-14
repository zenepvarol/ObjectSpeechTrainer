//
//  ARKitCameraInputSourceBindings.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 03/06/22.
//

#import <Foundation/Foundation.h>
#import "NPKit.h"
NS_ASSUME_NONNULL_BEGIN

NPBINDING DONTSTRIP void* MLKit_CreateDynamicImageInputSourceFeed_InitWithTexture(void* texturePtr, int x, int y, int width, int height);
NPBINDING DONTSTRIP void MLKit_DynamicImageInputSourceFeed_OnCameraFrameReceived(void* inputSourcePtr);


NS_ASSUME_NONNULL_END
