using System.Xml.Linq;

namespace ModulatorConfigurator.Services
{
    public class ModusFileParserService
    {
        public class PwmModulator
        {
            public string Alias { get; set; } = "";
            public string Location { get; set; } = "";
            public Dictionary<string, string> Parameters { get; set; } = new();
        }

        public class ModusParseResult
        {
            public List<PwmModulator> PwmModulators { get; set; } = new();
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
                
                // Find all Personality elements with template="mxs40pwm_ver2"
                var pwmPersonalities = doc.Descendants(ns + "Personality")
                    .Where(p => p.Attribute("template")?.Value == "mxs40pwm_ver2");

                foreach (var personality in pwmPersonalities)
                {
                    var block = personality.Element(ns + "Block");
                    if (block == null) continue;

                    var location = block.Attribute("location")?.Value ?? "";
                    
                    // Get the alias
                    var aliasElement = block.Element(ns + "Aliases")?.Element(ns + "Alias");
                    var alias = aliasElement?.Attribute("value")?.Value ?? "";

                    if (!string.IsNullOrEmpty(alias))
                    {
                        var modulator = new PwmModulator
                        {
                            Alias = alias,
                            Location = location
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
                                    modulator.Parameters[id] = value;
                                }
                            }
                        }

                        result.PwmModulators.Add(modulator);
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