using System.Text.Json.Serialization;

namespace ModulatorConfigurator.Models;

public class ModulatorConfiguration
{
    public List<Modulator> Modulators { get; set; } = new();
}

public class Modulator
{
    public string Name { get; set; } = "";
    public double Frequency { get; set; } = 1000.0;
    public double Deadtime { get; set; } = 0.0;
    public double DutyCycle { get; set; } = 50.0;
    public bool IsInverted { get; set; } = false;
    public List<Phase> Phases { get; set; } = new();
}

public class Phase
{
    public string Pwm { get; set; } = "";
    public double PhaseShift { get; set; } = 0.0;
    public bool GenerateIsr { get; set; } = false;
    public IsrTriggerSource IsrTriggerSource { get; set; } = IsrTriggerSource.PeriodMatch;
    public bool TriggerAdc { get; set; } = false;
    public AdcTriggerSource AdcTriggerSource { get; set; } = AdcTriggerSource.TR0;
}

public enum IsrTriggerSource
{
    PeriodMatch,
    Compare0Match,
    Compare1Match
}

public enum AdcTriggerSource
{
    TR0, TR1, TR2, TR3, TR4, TR5, TR6, TR7
}