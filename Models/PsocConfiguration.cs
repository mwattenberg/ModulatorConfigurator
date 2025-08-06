using System.Text.Json.Serialization;

namespace ModulatorConfigurator.Models;

public class PsocConfiguration
{
    public string ProjectName { get; set; } = "PsocProject";
    public string Version { get; set; } = "1.0.0";
    public List<Pin> Pins { get; set; } = new();
    public List<Component> Components { get; set; } = new();
    public ClockSettings ClockSettings { get; set; } = new();
    public List<Timer> Timers { get; set; } = new();
    public List<Uart> Uarts { get; set; } = new();
    public List<I2c> I2cs { get; set; } = new();
    public List<Spi> Spis { get; set; } = new();
}

public class Pin
{
    public string Name { get; set; } = "";
    public int Port { get; set; }
    public int Number { get; set; }
    public PinMode Mode { get; set; } = PinMode.Input;
    public PinDrive Drive { get; set; } = PinDrive.Standard;
    public bool PullUp { get; set; }
    public bool PullDown { get; set; }
    public string Comment { get; set; } = "";
}

public class Component
{
    public string Name { get; set; } = "";
    public ComponentType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> ConnectedPins { get; set; } = new();
}

public class ClockSettings
{
    public int MainClockFrequency { get; set; } = 24000000; // 24MHz default
    public bool UseExternalCrystal { get; set; } = false;
    public int ExternalCrystalFrequency { get; set; } = 24000000;
}

public class Timer
{
    public string Name { get; set; } = "";
    public int Period { get; set; } = 1000;
    public TimerMode Mode { get; set; } = TimerMode.Continuous;
    public bool EnableInterrupt { get; set; } = false;
    public string InterruptHandler { get; set; } = "";
}

public class Uart
{
    public string Name { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public ParityType Parity { get; set; } = ParityType.None;
    public int StopBits { get; set; } = 1;
    public string TxPin { get; set; } = "";
    public string RxPin { get; set; } = "";
}

public class I2c
{
    public string Name { get; set; } = "";
    public int ClockFrequency { get; set; } = 100000; // 100kHz
    public string SclPin { get; set; } = "";
    public string SdaPin { get; set; } = "";
    public bool IsMaster { get; set; } = true;
    public byte SlaveAddress { get; set; } = 0x50;
}

public class Spi
{
    public string Name { get; set; } = "";
    public int ClockFrequency { get; set; } = 1000000; // 1MHz
    public SpiMode Mode { get; set; } = SpiMode.Mode0;
    public string MosiPin { get; set; } = "";
    public string MisoPin { get; set; } = "";
    public string SclkPin { get; set; } = "";
    public string CsPin { get; set; } = "";
}

// Enums
public enum PinMode
{
    Input,
    Output,
    Bidirectional,
    Analog
}

public enum PinDrive
{
    Standard,
    HighDrive,
    OpenDrain
}

public enum ComponentType
{
    Led,
    Button,
    Potentiometer,
    TemperatureSensor,
    Custom
}

public enum TimerMode
{
    OneShot,
    Continuous,
    Pwm
}

public enum ParityType
{
    None,
    Even,
    Odd
}

public enum SpiMode
{
    Mode0, // CPOL=0, CPHA=0
    Mode1, // CPOL=0, CPHA=1
    Mode2, // CPOL=1, CPHA=0
    Mode3  // CPOL=1, CPHA=1
}