using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingInt : CmdSetting
    {
        public override string UsageParameters { get; } = "<number>";
        public virtual int LowerBound { get { return int.MinValue; } }
        public virtual int UpperBound { get { return int.MaxValue; } }
        UpdateResult CheckBound(int num)
        {
            var BoundInvalid =
                (UpperBound != int.MaxValue && num > UpperBound)
                || (LowerBound != int.MinValue && num < LowerBound);
            if (BoundInvalid)
            {
                string Message;
                if (LowerBound != int.MinValue && UpperBound != int.MaxValue)
                    Message = $"Invalid number. Number should be between {LowerBound} and {UpperBound}";
                else if (LowerBound != int.MinValue)
                    Message = $"Invalid number. Number should be above {LowerBound}";
                else
                    Message = $"Invalid number. Number should be below {UpperBound}";
                return new UpdateResult(false, this.Value, Message);
            }
            return new UpdateResult(true, num);
        }
        public override UpdateResult UpdateFromString(string input)
        {
            int num;
            if (int.TryParse(input.Trim(), out num))
                return CheckBound(num);
            else
                return new UpdateResult(false, this.Value, "Invalid number. Number should have no decimals.");
        }
        public override UpdateResult UpdateFromObject(object input)
        {
            if (input.GetType() == typeof(int))
                return CheckBound((int)input);
            else
                return new UpdateResult(false, Default, "Type should be an integer (number, no decimal)");
        }
    }
}
