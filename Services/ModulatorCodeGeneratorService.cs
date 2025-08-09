using ModulatorConfigurator.Models;
using System.Text;

namespace ModulatorConfigurator.Services
{
    public class ModulatorCodeGeneratorService
    {
        public class CodeGenerationResult
        {
            public string HeaderFile { get; set; } = "";
            public string SourceFile { get; set; } = "";
            public string HeaderFileName { get; set; } = "";
            public string SourceFileName { get; set; } = "";
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
        }   

        public CodeGenerationResult GenerateCode(List<Modulator> modulators)
        {
            try
            {
                // Determine base name from first modulator or use default
                var baseName = GetBaseName(modulators);
                
                var result = new CodeGenerationResult
                {
                    HeaderFile = GenerateHeaderFile(modulators, baseName),
                    SourceFile = GenerateSourceFile(modulators, baseName),
                    HeaderFileName = $"{baseName}.h",
                    SourceFileName = $"{baseName}.c",
                    Success = true
                };
                return result;
            }
            catch (Exception ex)
            {
                return new CodeGenerationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private string GetBaseName(List<Modulator> modulators)
        {
            if (modulators.Any() && !string.IsNullOrWhiteSpace(modulators.First().Name))
            {
                // Use first modulator name, sanitized for file names
                return SanitizeForFileName(modulators.First().Name.ToLower());
            }
            
            return "modulator_config";
        }

        private string SanitizeForFileName(string name)
        {
            // Replace spaces and special characters with underscores
            return name.Replace(" ", "_")
                      .Replace("-", "_")
                      .Replace(".", "_")
                      .ToLower();
        }

        private string GetModulatorPrefix(Modulator modulator)
        {
            if (!string.IsNullOrWhiteSpace(modulator.Name))
            {
                return modulator.Name.ToUpper().Replace(" ", "_").Replace("-", "_");
            }
            
            return "MODULATOR";
        }

        private string GenerateHeaderFile(List<Modulator> modulators, string baseName)
        {
            var sb = new StringBuilder();
            var guardName = $"{baseName.ToUpper()}_H";
            
            sb.AppendLine("/*");
            sb.AppendLine($" * {baseName} - Generated Header File");
            sb.AppendLine($" * Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(" */");
            sb.AppendLine();
            sb.AppendLine($"#ifndef {guardName}");
            sb.AppendLine($"#define {guardName}");
            sb.AppendLine();
            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("#include <stdbool.h>");
            sb.AppendLine();
            
            // Generate enums and defines
            GenerateEnumsAndDefines(sb, modulators);
            
            // Generate structures
            GenerateStructures(sb, modulators);
            
            // Generate function prototypes
            GenerateFunctionPrototypes(sb, modulators);
            
            sb.AppendLine();
            sb.AppendLine($"#endif /* {guardName} */");
            
            return sb.ToString();
        }
        
        private string GenerateSourceFile(List<Modulator> modulators, string baseName)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("/*");
            sb.AppendLine($" * {baseName} - Generated Source File");
            sb.AppendLine($" * Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(" */");
            sb.AppendLine();
            sb.AppendLine($"#include \"{baseName}.h\"");
            sb.AppendLine("#include <math.h>");
            sb.AppendLine();
            
            // Generate configuration instances
            GenerateConfigurationInstances(sb, modulators);
            
            // Generate initialization functions
            GenerateInitializationFunctions(sb, modulators);
            
            // Generate control functions
            GenerateControlFunctions(sb, modulators);
            
            // Generate interrupt handlers
            GenerateInterruptHandlers(sb, modulators);
            
            return sb.ToString();
        }
        
        private void GenerateEnumsAndDefines(StringBuilder sb, List<Modulator> modulators)
        {
            sb.AppendLine("/* Modulator Configuration Enums and Defines */");
            sb.AppendLine();
            
            // Generate individual mode enums for each modulator
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                sb.AppendLine($"typedef enum {{{prefix}_disabled, {prefix}_openLoop, {prefix}_currentControl}} {prefix}_mode_t;");
            }
            sb.AppendLine();
            
            // Phase enumeration - dynamically generate based on modulators
            var allPhases = new List<string>();
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        allPhases.Add($"{prefix}_{phase.Pwm.Replace("PWM_", "")}");
                    }
                }
            }
            
            if (allPhases.Any())
            {
                sb.AppendLine("typedef enum {");
                for (int i = 0; i < allPhases.Count; i++)
                {
                    sb.AppendLine($"    {allPhases[i]}{(i == allPhases.Count - 1 ? "" : ",")}");
                }
                sb.AppendLine("} modulator_phase_t;");
                sb.AppendLine();
            }
            
            // Define duty cycle limits for each modulator
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                sb.AppendLine($"/* {modulator.Name} Duty Cycle Limits */");
                sb.AppendLine($"#define {prefix}_MAX_DUTY 0.97f");
                sb.AppendLine($"#define {prefix}_MIN_DUTY 0.03f");
                sb.AppendLine();
            }
            
            // Generate ADC definitions for each phase
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine($"/* {modulator.Name} ADC Definitions */");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        var phaseName = phase.Pwm.Replace("PWM_", "").Replace("_INNER", "").Replace("_OUTER", "");
                        sb.AppendLine($"#define {prefix}_I_{phaseName} ({modulatorName}_config.{modulatorName}.ADC_res[0])");
                        sb.AppendLine($"#define {prefix}_VFC_{phaseName} ({modulatorName}_config.{modulatorName}.ADC_res[1])");
                    }
                }
                sb.AppendLine();
            }
        }
        
        private void GenerateStructures(StringBuilder sb, List<Modulator> modulators)
        {
            // Generate individual phase config structures for each modulator
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                sb.AppendLine($"/* {modulator.Name} Phase Configuration Structure */");
                sb.AppendLine($"typedef struct {prefix}_phaseConfig_t");
                sb.AppendLine("{");
                sb.AppendLine("    float duty;");
                sb.AppendLine("    uint16_t period;");
                sb.AppendLine("    uint32_t Iref;");
                sb.AppendLine("    int32_t compareValueOffset;");
                sb.AppendLine("    uint32_t ADC_res[2];");
                sb.AppendLine("    uint32_t currentFilt;");
                sb.AppendLine($"}} {prefix}_phaseConfig_t;");
                sb.AppendLine();
            }
            
            // Generate individual config structures for each modulator
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine($"/* {modulator.Name} Configuration Structure */");
                sb.AppendLine($"typedef struct {modulatorName}_config_t");
                sb.AppendLine("{");
                sb.AppendLine($"    {prefix}_phaseConfig_t {modulatorName};");
                sb.AppendLine("    float Kp;");
                sb.AppendLine("    uint32_t deadtimePos;");
                sb.AppendLine("    uint32_t deadtimeNeg;");
                sb.AppendLine($"    {prefix}_mode_t mode;");
                sb.AppendLine("    bool isRunning;");
                sb.AppendLine("    uint32_t fsw;");
                sb.AppendLine("    uint32_t clockFreq;");
                sb.AppendLine($"}} {modulatorName}_config_t;");
                sb.AppendLine();
                sb.AppendLine($"extern {modulatorName}_config_t {modulatorName}_config;");
                sb.AppendLine();
            }
        }
        
        private void GenerateFunctionPrototypes(StringBuilder sb, List<Modulator> modulators)
        {
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine($"/* {modulator.Name} Function Prototypes */");
                sb.AppendLine($"void {modulatorName}_init(void);");
                sb.AppendLine($"void {modulatorName}_startAll(void);");
                sb.AppendLine($"void {modulatorName}_stopAll(void);");
                sb.AppendLine($"void {modulatorName}_setFsw(uint32_t fsw);");
                sb.AppendLine($"void {modulatorName}_setDuty(modulator_phase_t phase, float duty);");
                sb.AppendLine($"void {modulatorName}_setDeadtime(uint8_t deadtime);");
                sb.AppendLine($"void {modulatorName}_setDeadtimeNeg(uint32_t deadtime);");
                sb.AppendLine($"void {modulatorName}_setDeadtimePos(uint32_t deadtime);");
                sb.AppendLine($"void {modulatorName}_setMode({prefix}_mode_t mode);");
                sb.AppendLine($"void {modulatorName}_setCurrent(float current);");
                sb.AppendLine();
                
                // Generate controller function prototypes for phases with ISR enabled
                foreach (var phase in modulator.Phases)
                {
                    if (phase.GenerateIsr && !string.IsNullOrEmpty(phase.Pwm))
                    {
                        var phaseName = phase.Pwm.Replace("PWM_", "").Replace("_INNER", "").Replace("_OUTER", "");
                        sb.AppendLine($"__attribute__ ((interrupt (\"IRQ\"))) __attribute__ ((section (\".sram\")))");
                        sb.AppendLine($"void {modulatorName}_{phaseName}_Controller(void);");
                    }
                }
                sb.AppendLine();
            }
        }
        
        private void GenerateConfigurationInstances(StringBuilder sb, List<Modulator> modulators)
        {
            sb.AppendLine("/* Static helper functions */");
            foreach (var modulator in modulators)
            {
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                sb.AppendLine($"static inline void {modulatorName}_init_DMA_ISR(void);");
                sb.AppendLine($"static inline void {modulatorName}_updatePhaseShift(void);");
            }
            sb.AppendLine();
            
            // Generate config instances for each modulator
            foreach (var modulator in modulators)
            {
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                sb.AppendLine($"/* {modulator.Name} Configuration Instance */");
                sb.AppendLine($"{modulatorName}_config_t {modulatorName}_config = {{.Kp = 1.0f}};");
                sb.AppendLine();
            }
        }
        
        private void GenerateInitializationFunctions(StringBuilder sb, List<Modulator> modulators)
        {
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine("/*");
                sb.AppendLine($" * Initialize {modulator.Name} configuration");
                sb.AppendLine(" * Sets the interrupt handler called by the ADC interrupt.");
                sb.AppendLine(" */");
                sb.AppendLine($"void {modulatorName}_init(void)");
                sb.AppendLine("{");
                sb.AppendLine($"    {modulatorName}_config.deadtimePos = 20;");
                sb.AppendLine($"    {modulatorName}_config.deadtimeNeg = 20;");
                
                var dutyCycle = string.IsNullOrEmpty(modulator.DutyCycle) ? "0.5f" : $"{modulator.DutyCycle}f";
                var frequency = string.IsNullOrEmpty(modulator.Frequency) ? "80e3" : modulator.Frequency;
                
                sb.AppendLine($"    {modulatorName}_config.{modulatorName}.duty = {dutyCycle};");
                sb.AppendLine($"    {modulatorName}_config.fsw = {frequency};");
                sb.AppendLine();
                sb.AppendLine($"    {modulatorName}_config.clockFreq = Cy_SysClk_ClkHfGetFrequency(3);");
                sb.AppendLine($"    {modulatorName}_config.{modulatorName}.compareValueOffset = 0;");
                sb.AppendLine();
                
                // Initialize PWM modules for this modulator
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_PWM_Init({phase.Pwm}_HW, {phase.Pwm}_NUM, &{phase.Pwm}_config);");
                        sb.AppendLine($"    Cy_TCPWM_PWM_Enable({phase.Pwm}_HW, {phase.Pwm}_NUM);");
                    }
                }
                sb.AppendLine();
                
                // Setup kill input
                sb.AppendLine("    // Set up the kill input");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_InputTriggerSetup({phase.Pwm}_HW, {phase.Pwm}_NUM, CY_TCPWM_INPUT_TR_STOP_OR_KILL, CY_TCPWM_INPUT_RISINGEDGE, CY_TCPWM_INPUT_TRIG(5));");
                    }
                }
                sb.AppendLine();
                
                // Setup swap functionality
                sb.AppendLine("    // Setup swap functionality for variable fsw");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_InputTriggerSetup({phase.Pwm}_HW, {phase.Pwm}_NUM, CY_TCPWM_INPUT_TR_INDEX_OR_SWAP, CY_TCPWM_INPUT_RISINGEDGE, CY_TCPWM_INPUT_TRIG(6));");
                    }
                }
                sb.AppendLine();
                
                // Setup start trigger
                sb.AppendLine("    // Setup start trigger for synchronized operation");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_InputTriggerSetup({phase.Pwm}_HW, {phase.Pwm}_NUM, CY_TCPWM_INPUT_TR_START, CY_TCPWM_INPUT_RISINGEDGE, CY_TCPWM_INPUT_TRIG(4));");
                    }
                }
                sb.AppendLine();
                
                sb.AppendLine($"    {modulatorName}_setFsw({modulatorName}_config.fsw);");
                sb.AppendLine($"    {modulatorName}_updatePhaseShift();");
                sb.AppendLine($"    {modulatorName}_init_DMA_ISR();");
                sb.AppendLine($"    {modulatorName}_setMode({prefix}_openLoop);");
                sb.AppendLine($"    {modulatorName}_setDeadtimePos({modulatorName}_config.deadtimePos);");
                sb.AppendLine($"    {modulatorName}_setDeadtimeNeg({modulatorName}_config.deadtimeNeg);");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
        
        private void GenerateControlFunctions(StringBuilder sb, List<Modulator> modulators)
        {
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine("/*");
                sb.AppendLine($" * Start {modulator.Name}");
                sb.AppendLine(" */");
                sb.AppendLine($"void {modulatorName}_startAll(void)");
                sb.AppendLine("{");
                sb.AppendLine("    Cy_TrigMux_SwTrigger(TRIG_OUT_MUX_10_TCPWM0_TR_IN4, CY_TRIGGER_TWO_CYCLES);");
                sb.AppendLine("    Cy_TrigMux_SwTrigger(TRIG_OUT_MUX_10_TCPWM0_TR_IN5, CY_TRIGGER_DEACTIVATE);");
                sb.AppendLine($"    {modulatorName}_config.isRunning = true;");
                sb.AppendLine("}");
                sb.AppendLine();
                
                sb.AppendLine("/*");
                sb.AppendLine($" * Stop {modulator.Name}");
                sb.AppendLine(" */");
                sb.AppendLine($"void {modulatorName}_stopAll(void)");
                sb.AppendLine("{");
                sb.AppendLine("    Cy_TrigMux_SwTrigger(TRIG_OUT_MUX_10_TCPWM0_TR_IN5, CY_TRIGGER_INFINITE);");
                sb.AppendLine($"    {modulatorName}_config.isRunning = false;");
                sb.AppendLine("}");
                sb.AppendLine();
                
                sb.AppendLine("/*");
                sb.AppendLine($" * Set {modulator.Name} switching frequency");
                sb.AppendLine(" */");
                sb.AppendLine($"void {modulatorName}_setFsw(uint32_t fsw)");
                sb.AppendLine("{");
                sb.AppendLine($"    {modulatorName}_config.{modulatorName}.period = {modulatorName}_config.clockFreq / (2*fsw); // (2*fsw because of center aligned modulation)");
                sb.AppendLine($"    {modulatorName}_config.fsw = fsw;");
                sb.AppendLine();
                
                // Set period for PWM modules
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_PWM_SetPeriod1({phase.Pwm}_HW, {phase.Pwm}_NUM, {modulatorName}_config.{modulatorName}.period);");
                    }
                }
                sb.AppendLine();
                
                sb.AppendLine("    // When we change the frequency we also need to recalculate the duty cycle");
                if (modulator.Phases.Any(p => !string.IsNullOrEmpty(p.Pwm)))
                {
                    var firstPhase = modulator.Phases.First(p => !string.IsNullOrEmpty(p.Pwm));
                    var phaseEnum = $"{prefix}_{firstPhase.Pwm.Replace("PWM_", "")}";
                    sb.AppendLine($"    {modulatorName}_setDuty({phaseEnum}, {modulatorName}_config.{modulatorName}.duty);");
                }
                sb.AppendLine("}");
                sb.AppendLine();
                
                sb.AppendLine("/*");
                sb.AppendLine($" * Set {modulator.Name} duty cycle for specific phase");
                sb.AppendLine(" */");
                sb.AppendLine($"void {modulatorName}_setDuty(modulator_phase_t phase, float duty)");
                sb.AppendLine("{");
                sb.AppendLine("    uint16_t compare;");
                sb.AppendLine();
                sb.AppendLine($"    if(duty > {prefix}_MAX_DUTY)");
                sb.AppendLine($"        duty = {prefix}_MAX_DUTY;");
                sb.AppendLine($"    else if(duty < {prefix}_MIN_DUTY)");
                sb.AppendLine($"        duty = {prefix}_MIN_DUTY;");
                sb.AppendLine();
                sb.AppendLine("    switch(phase)");
                sb.AppendLine("    {");
                
                // Generate case statements for each phase
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        var phaseEnum = $"{prefix}_{phase.Pwm.Replace("PWM_", "")}";
                        sb.AppendLine($"        case {phaseEnum}:");
                        sb.AppendLine($"            {modulatorName}_config.{modulatorName}.duty = duty;");
                        sb.AppendLine($"            compare = {modulatorName}_config.{modulatorName}.period - {modulatorName}_config.{modulatorName}.period * duty;");
                        sb.AppendLine($"            Cy_TCPWM_PWM_SetCompare0BufVal({phase.Pwm}_HW, {phase.Pwm}_NUM, compare + {modulatorName}_config.{modulatorName}.compareValueOffset);");
                        sb.AppendLine($"            break;");
                    }
                }
                
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    Cy_TrigMux_SwTrigger(TRIG_OUT_MUX_10_TCPWM0_TR_IN6, CY_TRIGGER_TWO_CYCLES);");
                sb.AppendLine("}");
                sb.AppendLine();
                
                // Generate deadtime functions
                sb.AppendLine($"void {modulatorName}_setDeadtimePos(uint32_t deadtime)");
                sb.AppendLine("{");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_PWM_PWMDeadTime({phase.Pwm}_HW, {phase.Pwm}_NUM, deadtime);");
                    }
                }
                sb.AppendLine($"    {modulatorName}_config.deadtimePos = deadtime;");
                sb.AppendLine("}");
                sb.AppendLine();
                
                sb.AppendLine($"void {modulatorName}_setDeadtimeNeg(uint32_t deadtime)");
                sb.AppendLine("{");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm))
                    {
                        sb.AppendLine($"    Cy_TCPWM_PWM_PWMDeadTimeN({phase.Pwm}_HW, {phase.Pwm}_NUM, deadtime);");
                    }
                }
                sb.AppendLine($"    {modulatorName}_config.deadtimeNeg = deadtime;");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
        
        private void GenerateInterruptHandlers(StringBuilder sb, List<Modulator> modulators)
        {
            foreach (var modulator in modulators)
            {
                var prefix = GetModulatorPrefix(modulator);
                var modulatorName = modulator.Name.ToLower().Replace(" ", "_");
                
                sb.AppendLine("/*");
                sb.AppendLine($" * {modulator.Name} Current Controller Helper Function");
                sb.AppendLine(" */");
                sb.AppendLine($"static inline void {modulatorName}_CurrentController(modulator_phase_t phase, int32_t err)");
                sb.AppendLine("{");
                sb.AppendLine("    float duty;");
                sb.AppendLine("    float VL;");
                sb.AppendLine("    float VIN_temp = 1.0f; // TODO: Replace with actual input voltage reading");
                sb.AppendLine();
                sb.AppendLine($"    VL = err * {modulatorName}_config.Kp;");
                sb.AppendLine("    if(VIN_temp > 0)");
                sb.AppendLine("        duty = (VIN_temp + VL) / 400.0f; // TODO: Adjust divisor based on VDC");
                sb.AppendLine("    else");
                sb.AppendLine("        duty = 1.0f + (VIN_temp + VL) / 400.0f;");
                sb.AppendLine();
                sb.AppendLine($"    {modulatorName}_setDuty(phase, duty);");
                sb.AppendLine("}");
                sb.AppendLine();
                
                // Generate interrupt handlers for phases with ISR enabled
                foreach (var phase in modulator.Phases)
                {
                    if (phase.GenerateIsr && !string.IsNullOrEmpty(phase.Pwm))
                    {
                        var phaseName = phase.Pwm.Replace("PWM_", "").Replace("_INNER", "").Replace("_OUTER", "");
                        var phaseEnum = $"{prefix}_{phase.Pwm.Replace("PWM_", "")}";
                        
                        sb.AppendLine("/*");
                        sb.AppendLine($" * {modulator.Name} {phaseName} Controller ISR");
                        sb.AppendLine(" * Function is called by interrupt to control the phase current and flying cap");
                        sb.AppendLine(" * voltage via a simple P controller.");
                        sb.AppendLine(" * Duty cycle is updated at the rate of the PWM.");
                        sb.AppendLine(" */");
                        sb.AppendLine("__attribute__ ((interrupt (\"IRQ\"))) __attribute__ ((section (\".sram\")))");
                        sb.AppendLine($"void {modulatorName}_{phaseName}_Controller(void)");
                        sb.AppendLine("{");
                        sb.AppendLine("    // elapsed_time_start(0); // Uncomment if using performance measurement");
                        sb.AppendLine();
                        sb.AppendLine($"    {modulatorName}_config.{modulatorName}.currentFilt = moving_average({prefix}_I_{phaseName});");
                        sb.AppendLine();
                        sb.AppendLine($"    int32_t err = {modulatorName}_config.{modulatorName}.Iref - {modulatorName}_config.{modulatorName}.currentFilt;");
                        sb.AppendLine($"    {modulatorName}_CurrentController({phaseEnum}, err);");
                        
                        if (phase.TriggerAdc)
                        {
                            sb.AppendLine($"    Cy_DMA_Channel_ClearInterrupt(DMA_{phaseName}_HW, DMA_{phaseName}_CHANNEL);");
                        }
                        
                        sb.AppendLine();
                        sb.AppendLine("    // elapsed_time_stop(0); // Uncomment if using performance measurement");
                        sb.AppendLine("}");
                        sb.AppendLine();
                    }
                }
                
                // Generate placeholder helper functions
                sb.AppendLine("/*");
                sb.AppendLine($" * {modulator.Name} helper function implementations - customize as needed");
                sb.AppendLine(" */");
                sb.AppendLine($"static inline void {modulatorName}_init_DMA_ISR(void)");
                sb.AppendLine("{");
                sb.AppendLine("    // TODO: Initialize DMA and ISR configurations");
                sb.AppendLine("    // Configure DMA channels for ADC data transfer");
                sb.AppendLine("    // Set up interrupt priorities and handlers");
                sb.AppendLine("}");
                sb.AppendLine();
                
                sb.AppendLine($"static inline void {modulatorName}_updatePhaseShift(void)");
                sb.AppendLine("{");
                sb.AppendLine("    // TODO: Implement phase shift calculations");
                sb.AppendLine("    // Set initial phase shifts for synchronized operation");
                foreach (var phase in modulator.Phases)
                {
                    if (!string.IsNullOrEmpty(phase.Pwm) && !string.IsNullOrEmpty(phase.PhaseShift))
                    {
                        sb.AppendLine($"    // Set phase shift for {phase.Pwm}: {phase.PhaseShift}");
                    }
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
    }
}