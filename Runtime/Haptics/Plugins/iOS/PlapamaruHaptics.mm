#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator *PlapamaruHapticsGenerators[5];

extern "C" {

void PlapamaruHaptics_Impact(int type) {
    if (@available(iOS 13.0, *)) {
        if (type < 0 || type > 4) {
            type = 4;
        }

        UIImpactFeedbackGenerator *generator = PlapamaruHapticsGenerators[type];
        if (generator == nil) {
            UIImpactFeedbackStyle styles[5] = {
                UIImpactFeedbackStyleHeavy,
                UIImpactFeedbackStyleLight,
                UIImpactFeedbackStyleMedium,
                UIImpactFeedbackStyleRigid,
                UIImpactFeedbackStyleSoft
            };
            generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:styles[type]];
            PlapamaruHapticsGenerators[type] = generator;
        }

        [generator impactOccurred];
        [generator prepare];
    }
}

}
