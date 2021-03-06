﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GraphPartition.Gui.GraphCreator;
using Graphs.EmbeddingInPlane;
using Optimizations;
using Utils.UiUtils;
using Utils.UiUtils.CustomUi.Creator;
using Utils.UiUtils.CustomUi.CustomType;

// ReSharper disable once CheckNamespace
namespace GraphPartition.Gui.MainApplication
{
    public partial class MainWindow : Window
    {
        private OptimizationType OptimizationType { get; set; }


        private void OptimizationTypeChanged(OptimizationType optimizationType)
        {
            OptimizationType = optimizationType;
            StackPanel settingsStackPanel;
            switch (optimizationType)
            {
                case OptimizationType.Genetic:
                    settingsStackPanel = InitGeneticSettings();
                    break;
                case OptimizationType.BranchAndBound:
                    settingsStackPanel = InitBranchAndBoundSettings();
                    break;
                case OptimizationType.LocalSearch:
                    settingsStackPanel = InitLocalSearchSettings();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optimizationType), optimizationType, null);
            }

            settingsStackPanel.Children.Add(ShowVisual());
            settingsStackPanel.Children.Add(InputGraphBullet());
            settingsStackPanel.Children.Add(CreateGraphButton());
            settingsStackPanel.Children.Add(OutputPathBullet());

            SettingsViewer.Content = settingsStackPanel;
        }

        private UIElement ShowVisual()
        {
            var chooser = RadioButtonChooser.Create(Dispatcher, new[] {true, false}, b => b.ToString(), OnVisualChanged);
            chooser.Orientation = Orientation.Horizontal;
            var text = TextBlockCreator.RegularTextBlock("Show Visual:").WithBullet();
            var stackPanel = GuiExtensions.CreateStackPanel(text, chooser);
            stackPanel.Orientation = Orientation.Horizontal;
            (chooser.Children[0] as RadioButton).IsChecked = true;
            return stackPanel;
        }

        public bool Visual = true;
        
        private void OnVisualChanged(bool visual)
        {
            Visual = visual;
        }

        private UIElement OutputPathBullet()
        {
            Predicate<string> validityCheck = path => Directory.Exists(path);
            Action<string> onPathChanged = path => OutputResultPath = path;
            var textBox = InteractiveTextBox.Create(OutputResultPath, validityCheck, onPathChanged, Dispatcher);
            return HorizontalContainerStrecherd.Create()
                .AddLeft(TextBlockCreator.RegularTextBlock("Result Output Folder").WithBullet())
                .AddRight(ButtonCreator.Create("...", () => DialogCreator.ChooseFolder(path => textBox.Text = path)))
                .AsDock(textBox);
        }


        private TextBox InputGraphTextBox { get; set; }

        private UIElement InputGraphBullet()
        {
            Predicate<string> validityCheck = path => File.Exists(path) && GraphEmbedding.CanParse(path);
            Action<string> onPathChanged = path =>
            {
                InputGraphPath = path;
                SetGraph(GraphEmbedding.FromText(File.ReadLines(path)));
            };
            var textBox = InteractiveTextBox.Create(InputGraphPath, validityCheck, onPathChanged, Dispatcher);
            InputGraphTextBox = textBox;
            return HorizontalContainerStrecherd.Create()
                .AddLeft(TextBlockCreator.RegularTextBlock("Input Graph").WithBullet())
                .AddRight(ButtonCreator.Create("...", () => DialogCreator.ChooseFile(path => textBox.Text = path)))
                .AsDock(textBox);
        }

        private UIElement CreateGraphButton() => ButtonCreator.Create("Create Graph", UserCreateGraphWindow);
        private void UserCreateGraphWindow()
        {
            var window = new GraphCreatorWindow(path => InputGraphTextBox.Text = path, NodeBrush, NumBrush, LineBrush, PenLineCap);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
    }
}