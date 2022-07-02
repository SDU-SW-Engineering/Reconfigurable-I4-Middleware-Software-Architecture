using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json;
using Orchestrator.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orchestrator.Adapter.RecipeInterpretation
{
    public class RecipeInterpreter
    {
        public RecipeInterpreter()
        {

        }

        public static Recipe ConvertFileToRecipe(II4Logger log, String filename)
        {
            Recipe recipe = null;
            try
            {
                using (StreamReader r = new StreamReader(filename))
                {
                    string json = r.ReadToEnd();
                    recipe = JsonConvert.DeserializeObject<Recipe>(json);
                }
            }
            catch (ArgumentException ex) { ThrowFileException(log, ex, filename); }
            catch (FileNotFoundException ex) { ThrowFileException(log, ex, filename); }
            catch (DirectoryNotFoundException ex) { ThrowFileException(log, ex, filename); }
            catch (IOException ex) { ThrowFileException(log, ex, filename); }
            catch (JsonException ex) 
            {
                log.LogError(typeof(RecipeInterpreter), ex, "The file with filename {filename} could not be interpreted, " +
                        "because of exception: {exceptionMessage}", filename, ex.Message);
                throw new RecipeNotInterpretedException($"The Recipe could not be interpreted -> {ex.Message}", ex);
            }
            return recipe;
        }
        public static Recipe ConvertMessageToRecipe(II4Logger log, String message)
        {
            Recipe recipe = null;
            try
            {

                    recipe = JsonConvert.DeserializeObject<Recipe>(message);
            }
            catch (JsonException ex)
            {
                log.LogError(typeof(RecipeInterpreter), ex, "The message {message} could not be interpreted, " +
                        "because of exception: {exceptionMessage}", message, ex.Message);
                throw new RecipeNotInterpretedException($"The Recipe could not be interpreted -> {ex.Message}", ex);
            }
            return recipe;
        }

        private static void ThrowFileException(II4Logger log, Exception ex, String filename)
        {
            log.LogError(typeof(RecipeInterpreter), ex, "The file with filename {filename} could not be found, " +
                        "because of exception: {exceptionMessage}", filename, ex.Message);
            throw new RecipeNotInterpretedException($"The Recipe could not be interpreted -> {ex.Message}", ex);
        }
    }
}
