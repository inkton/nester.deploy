using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nester.Views
{
    public interface IAppsHolder
    {
        ObservableCollection<Admin.App> Apps { get; set; }
    }
}
