using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    class UpdateResult
    {
        public bool Valid;
        public object NewValue;
        public string Message = "";
        public UpdateResult(bool Valid, object NewValue)
        {
            this.Valid = Valid;
            this.NewValue = NewValue;
        }

        public UpdateResult(bool Valid, object NewValue, string Message)
        {
            this.Valid = Valid;
            this.NewValue = NewValue;
            this.Message = Message;
        }
    }
}
