using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts.Configuration;
using SiGen.ViewModels;
using System;

namespace SiGen.Services
{
    public class ViewModelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public LayoutDocumentViewModel CreateLayoutDocumentViewModel(string? filepath, InstrumentLayoutConfiguration configuration, string? templateName = null)
        {
            string title = templateName ??
                (!string.IsNullOrEmpty(filepath) ? System.IO.Path.GetFileNameWithoutExtension(filepath) : Lang.Resources.NewDocumentName);

            return ActivatorUtilities.CreateInstance<LayoutDocumentViewModel>(
                serviceProvider,
                title,
                filepath ?? string.Empty,
                configuration);
        }
    }
}
