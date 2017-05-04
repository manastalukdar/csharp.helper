using System;

namespace csharp.helper.Utility
{
    public class EnvVar
    {
        #region Public Methods

        /// <summary>
        /// Gets the env var value.
        /// </summary>
        /// <param name="envVar">The env var.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">envVar</exception>
        public string GetEnvVarValue(string envVar)
        {
            if (envVar == null)
            {
                throw new ArgumentNullException("envVar");
            }

            var envVarValue = Environment.GetEnvironmentVariable(envVar);
            return envVarValue;
        }

        #endregion Public Methods
    }
}