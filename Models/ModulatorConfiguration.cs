using System.Text.Json.Serialization;

namespace ModulatorConfigurator.Models;

public class ModulatorConfiguration
{
    public List<Modulator> Modulators { get; set; } = new();
}

public class Modulator
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Deadtime { get; set; } = "";
    public string DutyCycle { get; set; } = "";
    public int GroupNumber { get; set; } = 0; // 0, 1, or 2
    public string TriggerStart { get; set; } = "";
    public string TriggerStop { get; set; } = "";
    public string TriggerSwap { get; set; } = "";
    public List<Phase> Phases { get; set; } = new();
}

public class Phase
{
    public int Id { get; set; }
    public string Alias { get; set; } = "";
    public string Pwm { get; set; } = "";
    public string PhaseShift { get; set; } = "";
    public bool GenerateIsr { get; set; } = false;
    public string IsrTriggerSource { get; set; } = "Period match";
    public bool TriggerAdc { get; set; } = false;
    public string AdcTriggerSource { get; set; } = "TR0";
    public PwmAlignment Alignment { get; set; } = PwmAlignment.CenterAligned;
    public int GroupNumber { get; set; } = 0; // 0, 1, or 2
    public bool IsInverted { get; set; } = false;
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

public enum PwmAlignment
{
    LeftAligned,
    RightAligned,
    CenterAligned
}

// Type aliases for compatibility with existing code
public class ModulatorModel : Modulator { }
public class PhaseModel : Phase { }