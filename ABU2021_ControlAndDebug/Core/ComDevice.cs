using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    interface ComDevice
    {
        #region Property
        bool IsConnected { get; }
        #endregion


        #region Method
        Task Connect();
        void Disconnect();


        Task SendMsgAsync(SendDataMsg msg);
        /// <summary>
        /// interfacetなのでasync宣言できない
        /// </summary>
        /// <returns></returns>
        Task<ReceiveDataMsg> ReadMsgAsync();
        #endregion
    }
}
