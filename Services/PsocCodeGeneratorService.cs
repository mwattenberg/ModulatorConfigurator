using ModulatorConfigurator.Models;
using System.Text;

namespace ModulatorConfigurator.Services
{
    public class PsocCodeGeneratorService
    {
        public string GenerateHeaderFile(PsocConfiguration config)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("/*");
            sb.AppendLine($" * {config.ProjectName} - Generated Header File");
            sb.AppendLine($" * Version: {config.Version}");
            sb.AppendLine($" * Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(" */");
            sb.AppendLine();
            sb.AppendLine("#ifndef PSOC_CONFIG_H");
            sb.AppendLine("#define PSOC_CONFIG_H");
            sb.AppendLine();
            sb.AppendLine("#include <project.h>");
            sb.AppendLine();
            
            // Clock configuration
            GenerateClockConfig(sb, config.ClockSettings);
            
            // Pin definitions
            GeneratePinDefinitions(sb, config.Pins);
            
            // Timer definitions
            GenerateTimerDefinitions(sb, config.Timers);
            
            // UART definitions
            GenerateUartDefinitions(sb, config.Uarts);
            
            // I2C definitions
            GenerateI2cDefinitions(sb, config.I2cs);
            
            // SPI definitions
            GenerateSpiDefinitions(sb, config.Spis);
            
            // Function prototypes
            GenerateFunctionPrototypes(sb, config);
            
            sb.AppendLine();
            sb.AppendLine("#endif // PSOC_CONFIG_H");
            
            return sb.ToString();
        }
        
        public string GenerateSourceFile(PsocConfiguration config)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("/*");
            sb.AppendLine($" * {config.ProjectName} - Generated Source File");
            sb.AppendLine($" * Version: {config.Version}");
            sb.AppendLine($" * Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(" */");
            sb.AppendLine();
            sb.AppendLine("#include \"psoc_config.h\"");
            sb.AppendLine();
            
            // Initialization function
            GenerateInitFunction(sb, config);
            
            // Timer interrupt handlers
            GenerateTimerInterruptHandlers(sb, config.Timers);
            
            // Utility functions
            GenerateUtilityFunctions(sb, config);
            
            return sb.ToString();
        }
        
        private void GenerateClockConfig(StringBuilder sb, ClockSettings clockSettings)
        {
            sb.AppendLine("/* Clock Configuration */");
            sb.AppendLine($"#define MAIN_CLOCK_FREQ_HZ    {clockSettings.MainClockFrequency}u");
            
            if (clockSettings.UseExternalCrystal)
            {
                sb.AppendLine($"#define EXTERNAL_CRYSTAL_FREQ_HZ    {clockSettings.ExternalCrystalFrequency}u");
                sb.AppendLine("#define USE_EXTERNAL_CRYSTAL    1");
            }
            else
            {
                sb.AppendLine("#define USE_EXTERNAL_CRYSTAL    0");
            }
            
            sb.AppendLine();
        }
        
        private void GeneratePinDefinitions(StringBuilder sb, List<Pin> pins)
        {
            if (pins.Count == 0) return;
            
            sb.AppendLine("/* Pin Definitions */");
            
            foreach (var pin in pins)
            {
                var pinName = pin.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"#define {pinName}_PORT    P{pin.Port}");
                sb.AppendLine($"#define {pinName}_PIN     {pin.Number}");
                sb.AppendLine($"#define {pinName}_MASK    (1u << {pin.Number})");
                
                if (!string.IsNullOrEmpty(pin.Comment))
                {
                    sb.AppendLine($"// {pin.Comment}");
                }
                
                sb.AppendLine();
            }
        }
        
        private void GenerateTimerDefinitions(StringBuilder sb, List<Models.Timer> timers)
        {
            if (timers.Count == 0) return;
            
            sb.AppendLine("/* Timer Definitions */");
            
            foreach (var timer in timers)
            {
                string timerName = timer.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"#define {timerName}_PERIOD    {timer.Period}u");
                
                if (timer.EnableInterrupt)
                {
                    sb.AppendLine($"#define {timerName}_INTERRUPT_ENABLED    1");
                }
            }
            
            sb.AppendLine();
        }
        
        private void GenerateUartDefinitions(StringBuilder sb, List<Uart> uarts)
        {
            if (uarts.Count == 0) return;
            
            sb.AppendLine("/* UART Definitions */");
            
            foreach (var uart in uarts)
            {
                var uartName = uart.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"#define {uartName}_BAUD_RATE    {uart.BaudRate}u");
                sb.AppendLine($"#define {uartName}_DATA_BITS    {uart.DataBits}u");
                sb.AppendLine($"#define {uartName}_STOP_BITS    {uart.StopBits}u");
            }
            
            sb.AppendLine();
        }
        
        private void GenerateI2cDefinitions(StringBuilder sb, List<I2c> i2cs)
        {
            if (i2cs.Count == 0) return;
            
            sb.AppendLine("/* I2C Definitions */");
            
            foreach (var i2c in i2cs)
            {
                var i2cName = i2c.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"#define {i2cName}_CLOCK_FREQ    {i2c.ClockFrequency}u");
                
                if (!i2c.IsMaster)
                {
                    sb.AppendLine($"#define {i2cName}_SLAVE_ADDR    0x{i2c.SlaveAddress:X2}");
                }
            }
            
            sb.AppendLine();
        }
        
        private void GenerateSpiDefinitions(StringBuilder sb, List<Spi> spis)
        {
            if (spis.Count == 0) return;
            
            sb.AppendLine("/* SPI Definitions */");
            
            foreach (var spi in spis)
            {
                var spiName = spi.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"#define {spiName}_CLOCK_FREQ    {spi.ClockFrequency}u");
                sb.AppendLine($"#define {spiName}_MODE    {(int)spi.Mode}u");
            }
            
            sb.AppendLine();
        }
        
        private void GenerateFunctionPrototypes(StringBuilder sb, PsocConfiguration config)
        {
            sb.AppendLine("/* Function Prototypes */");
            sb.AppendLine("void PsocConfig_Init(void);");
            
            foreach (var timer in config.Timers.Where(t => t.EnableInterrupt))
            {
                string timerName = timer.Name.Replace(" ", "_");
                sb.AppendLine($"CY_ISR_PROTO({timerName}_ISR);");
            }
            
            foreach (var pin in config.Pins.Where(p => p.Mode == PinMode.Output))
            {
                string pinName = pin.Name.Replace(" ", "_");
                sb.AppendLine($"void {pinName}_Write(uint8 value);");
            }
            
            foreach (var pin in config.Pins.Where(p => p.Mode == PinMode.Input))
            {
                string pinName = pin.Name.Replace(" ", "_");
                sb.AppendLine($"uint8 {pinName}_Read(void);");
            }
            
            sb.AppendLine();
        }
        
        private void GenerateInitFunction(StringBuilder sb, PsocConfiguration config)
        {
            sb.AppendLine("/*");
            sb.AppendLine(" * Initialize all PSOC components");
            sb.AppendLine(" */");
            sb.AppendLine("void PsocConfig_Init(void)");
            sb.AppendLine("{");
            sb.AppendLine("    /* Enable global interrupts */");
            sb.AppendLine("    CyGlobalIntEnable;");
            sb.AppendLine();
            
            // Initialize timers
            foreach (var timer in config.Timers)
            {
                string timerName = timer.Name.Replace(" ", "_");
                sb.AppendLine($"    /* Initialize {timer.Name} */");
                sb.AppendLine($"    {timerName}_Start();");
                
                if (timer.EnableInterrupt)
                {
                    sb.AppendLine($"    {timerName}_ISR_StartEx({timerName}_ISR);");
                }
                
                sb.AppendLine();
            }
            
            // Initialize UARTs
            foreach (var uart in config.Uarts)
            {
                string uartName = uart.Name.Replace(" ", "_");
                sb.AppendLine($"    /* Initialize {uart.Name} */");
                sb.AppendLine($"    {uartName}_Start();");
                sb.AppendLine();
            }
            
            // Initialize I2C
            foreach (var i2c in config.I2cs)
            {
                string i2cName = i2c.Name.Replace(" ", "_");
                sb.AppendLine($"    /* Initialize {i2c.Name} */");
                sb.AppendLine($"    {i2cName}_Start();");
                sb.AppendLine();
            }
            
            // Initialize SPI
            foreach (var spi in config.Spis)
            {
                string spiName = spi.Name.Replace(" ", "_");
                sb.AppendLine($"    /* Initialize {spi.Name} */");
                sb.AppendLine($"    {spiName}_Start();");
                sb.AppendLine();
            }
            
            // Initialize pins
            foreach (var pin in config.Pins)
            {
                var pinName = pin.Name.ToUpper().Replace(" ", "_");
                sb.AppendLine($"    /* Initialize {pin.Name} */");
                
                if (pin.Mode == PinMode.Output)
                {
                    sb.AppendLine($"    {pinName}_PORT_DR &= ~{pinName}_MASK; // Set initial state to low");
                }
                
                sb.AppendLine();
            }
            
            sb.AppendLine("}");
            sb.AppendLine();
        }
        
        private void GenerateTimerInterruptHandlers(StringBuilder sb, List<Models.Timer> timers)
        {
            foreach (var timer in timers.Where(t => t.EnableInterrupt))
            {
                string timerName = timer.Name.Replace(" ", "_");
                var handlerName = !string.IsNullOrEmpty(timer.InterruptHandler) 
                    ? timer.InterruptHandler 
                    : $"{timerName}_Handler";
                
                sb.AppendLine($"/*");
                sb.AppendLine($" * {timer.Name} interrupt service routine");
                sb.AppendLine($" */");
                sb.AppendLine($"CY_ISR({timerName}_ISR)");
                sb.AppendLine("{");
                sb.AppendLine($"    /* Clear the interrupt */");
                sb.AppendLine($"    {timerName}_ReadStatusRegister();");
                sb.AppendLine();
                sb.AppendLine($"    /* Call user handler */");
                sb.AppendLine($"    {handlerName}();");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
        
        private void GenerateUtilityFunctions(StringBuilder sb, PsocConfiguration config)
        {
            // Generate pin write functions
            foreach (var pin in config.Pins.Where(p => p.Mode == PinMode.Output))
            {
                string pinName = pin.Name.Replace(" ", "_");
                var pinNameUpper = pin.Name.ToUpper().Replace(" ", "_");
                
                sb.AppendLine($"/*");
                sb.AppendLine($" * Write to {pin.Name}");
                sb.AppendLine($" */");
                sb.AppendLine($"void {pinName}_Write(uint8 value)");
                sb.AppendLine("{");
                sb.AppendLine($"    if (value)");
                sb.AppendLine($"        {pinNameUpper}_PORT_DR |= {pinNameUpper}_MASK;");
                sb.AppendLine($"    else");
                sb.AppendLine($"        {pinNameUpper}_PORT_DR &= ~{pinNameUpper}_MASK;");
                sb.AppendLine("}");
                sb.AppendLine();
            }
            
            // Generate pin read functions
            foreach (var pin in config.Pins.Where(p => p.Mode == PinMode.Input))
            {
                string pinName = pin.Name.Replace(" ", "_");
                var pinNameUpper = pin.Name.ToUpper().Replace(" ", "_");
                
                sb.AppendLine($"/*");
                sb.AppendLine($" * Read from {pin.Name}");
                sb.AppendLine($" */");
                sb.AppendLine($"uint8 {pinName}_Read(void)");
                sb.AppendLine("{");
                sb.AppendLine($"    return ({pinNameUpper}_PORT_PS & {pinNameUpper}_MASK) ? 1u : 0u;");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
    }
}