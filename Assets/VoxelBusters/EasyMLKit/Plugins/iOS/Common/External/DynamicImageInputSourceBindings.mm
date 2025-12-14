//
//  ARKitCameraInputSourceBindings.m
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 03/06/22.
//

#import "DynamicImageInputSourceBindings.h"
#import "DynamicImageInputSourceFeed.h"

// C bindings
NPBINDING DONTSTRIP void* MLKit_CreateDynamicImageInputSourceFeed_InitWithTexture(void* texturePtr, int x, int y, int width, int height)
{
    id<MTLTexture> texture = (__bridge id<MTLTexture>)texturePtr;
    return (__bridge_retained void*)[[DynamicImageInputSourceFeed alloc] initWithTexture:texture withSelectedRegion:CGRectMake(x, y, width, height)];
}

NPBINDING DONTSTRIP void MLKit_DynamicImageInputSourceFeed_OnCameraFrameReceived(void* inputSourcePtr)
{
    DynamicImageInputSourceFeed*     inputSource     = (__bridge DynamicImageInputSourceFeed*)inputSourcePtr;
    [inputSource onCameraFrameReceived];
}
