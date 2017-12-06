using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingInt : CmdSetting<int>
    {
        public override string UsageParameters { get; } = "<number>";
        public virtual int LowerBound { get { return int.MinValue; } }
        public virtual int UpperBound { get { return int.MaxValue; } }
        UpdateResult<int> CheckBound(int num)
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
                return new UpdateResult<int>(false, this.Value, Message);
            }
            return new UpdateResult<int>(true, num);
        }
        public override UpdateResult<int> UpdateFromString(string input)
        {
            int num;
            if (int.TryParse(input.Trim(), out num))
                return CheckBound(num);
            else
                return new UpdateResult<int>(false, this.Value, "Invalid number. Number should have no decimals.");
        }
        public override UpdateResult<int> UpdateFromObject(object input)
        {
            if (input is int)
                return CheckBound((int)input);
            else
                return new UpdateResult<int>(false, Default, "Type should be an integer (number, no decimal)");
        }
    }
}
