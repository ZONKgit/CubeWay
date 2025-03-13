using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeWay.UI.Core
{
    public interface IGuiWindow
    {
        bool IsVisible { get; set; }
        void Render();
    }

}
