using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Monopoly.RLHandlers;
using System.Windows.Media;

namespace Monopoly
{
    public partial class App : Application
    {
        public RLEnvironment app = new RLEnvironment();
    }
}
