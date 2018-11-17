using System;
using System.Collections.Generic;
using System.Text;

namespace GFXExtract
{
    public class FrameTypeHelper
    {
        public static bool IsFrameTypeDirectionless(FrameType frameType) {
            switch (frameType) {
                case FrameType.Sleep:
                    return true;
                default:
                    return false;
            }
        }
    }
}
