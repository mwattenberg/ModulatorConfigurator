using System.Xml.Linq;
using ModulatorConfigurator.Models;

namespace ModulatorConfigurator.Services
{
    public class ModusFileParserService
    {
        public class ModusParseResult
        {
            public List<Phase> Phases { get; set; } = new();
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
        }

        public ModusParseResult ParseModusFile(string xmlContent)
        {
            var result = new ModusParseResult();
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var ns = XNamespace.Get("http://cypress.com/xsd/cydesignfile_v5");
                var pwmPersonalities = doc.Descendants(ns + "Personality")
                    .Where(p => p.Attribute("template")?.Value == "mxs40pwm_ver2");

                int nextPhaseId = 1;
                foreach (var personality in pwmPersonalities)
                {
                    var block = personality.Element(ns + "Block");
                    if (block == null) continue;

                    var location = block.Attribute("location")?.Value ?? "";
                    var aliasElement = block.Element(ns + "Aliases")?.Element(ns + "Alias");
                    var alias = aliasElement?.Attribute("value")?.Value ?? "";

                    if (!string.IsNullOrEmpty(alias))
                    {
                        var phase = new Phase
                        {
                            Id = nextPhaseId++,
                            Alias = alias,
                            Pwm = location
                        };

                        // Extract key parameters
                        var parameters = personality.Element(ns + "Parameters")?.Elements(ns + "Param");
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                var id = param.Attribute("id")?.Value;
                                var value = param.Attribute("value")?.Value;
                                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(value))
                                {
                                    // Map known parameters to Phase fields
                                    switch (id)
                                    {
                                        case "PhaseShift":
                                            phase.PhaseShift = value;
                                            break;
                                        case "IsInverted":
                                            phase.IsInverted = value == "true";
                                            break;
                                        case "GroupNumber":
                                            if (int.TryParse(value, out int groupNum))
                                                phase.GroupNumber = groupNum;
                                            break;
                                        case "Alignment":
                                            if (Enum.TryParse<PwmAlignment>(value, out var align))
                                                phase.Alignment = align;
                                            break;
                                        // Add more mappings as needed
                                    }
                                }
                            }
                        }
                        result.Phases.Add(phase);
                    }
                }
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error parsing .modus file: {ex.Message}";
            }
            return result;
        }
    }
}