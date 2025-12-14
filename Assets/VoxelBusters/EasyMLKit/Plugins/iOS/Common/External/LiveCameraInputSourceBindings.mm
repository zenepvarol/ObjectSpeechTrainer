//
//  ImageInputSourceFeedBindings.m
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 03/06/22.
//

#import "LiveCameraInputSourceBindings.h"
#import "LiveCameraInputSourceFeed.h"

NPBINDING DONTSTRIP void* MLKit_LiveCameraInputSourceFeed_Init(BOOL isFrontFacingMode, BOOL enableFlashLight, NPSize maxResolution, NPUnityRect viewport)
{
    return (__bridge_retained void*)[[LiveCameraInputSourceFeed alloc] initWithOptions: isFrontFacingMode withFlashLight:enableFlashLight
                                                                          withMaxResolution: CGSizeMake(maxResolution.width, maxResolution.height)
                                                                          withViewport:CGRectMake(viewport.x, viewport.y, viewport.width, viewport.height)];
}
