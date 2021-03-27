using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Models.EnumModels
{
    public enum Instrument
    {
        guitar,
        bass,
        keys,
        drums,
        singing
    }

    public class InstrumentDictionaries
    {
        public static Dictionary<Instrument, string> InstrumentToString = new Dictionary<Instrument, string>()
        {
            { Instrument.guitar, "Гитара" },
            { Instrument.bass, "Бас" },
            { Instrument.keys, "Клавиши" },
            { Instrument.drums, "Барабаны" },
            { Instrument.singing, "Пение" },
        };

        public static Dictionary<string, Instrument> StringToInstrument = new Dictionary<string, Instrument>()
        {
            { "Гитара", Instrument.guitar },
            { "Бас", Instrument.bass },
            { "Клавиши", Instrument.keys },
            { "Барабаны", Instrument.drums },
            { "Пение", Instrument.singing },
        };
    }
}
