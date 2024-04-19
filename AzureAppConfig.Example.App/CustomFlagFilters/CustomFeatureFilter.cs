using Microsoft.FeatureManagement;

namespace AzureAppConfig.Example.App.CustomFlagFilters
{
    [FilterAlias("ExampleCustomFilter")] // How we will refer to the filter in configuration
    public class CustomFeatureFilter : IFeatureFilter
    {
        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            bool enabled = false;

            var settings = context.Parameters.Get<CustomFilterSettings>();

            if (settings != null)
                enabled = DateTime.Now.DayOfWeek == settings.DayOfWeek;
            
            return Task.FromResult(enabled);
            
        }
    }
}