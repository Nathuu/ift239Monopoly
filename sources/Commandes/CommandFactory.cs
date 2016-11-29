﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApplication1.sources.Commandes;

namespace WpfApplication1.sources
{
    public class CommandFactory
    {
        private static CommandFactory instance;
        private CommandFactory()
        {

        }

        public static CommandFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new CommandFactory();
                return instance;
            }
        }

        public CommandFinTour CreateCommandFinTour()
        {
            return new CommandFinTour();
        }

        public CommandSauvegarde CreateCommandSauvegarde()
        {
            return new CommandSauvegarde();
        }
    }
}
