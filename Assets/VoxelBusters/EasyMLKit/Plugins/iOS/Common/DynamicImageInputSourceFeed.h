//
//  ARKitCameraInputSourceFeed.h
//  NativeMLKitiOS
//
//  Created by Ayyappa J on 27/05/22.
//

#import <Foundation/Foundation.h>
#import "IVisionInputSourceFeed.h"
#import <ARKit/ARSession.h>

NS_ASSUME_NONNULL_BEGIN

@interface DynamicImageInputSourceFeed : NSObject<IVisionInputSourceFeed>
-(id)initWithTexture:(id<MTLTexture>)texture withSelectedRegion:(CGRect) selectedRegion;
-(void) onCameraFrameReceived;
@end

NS_ASSUME_NONNULL_END
