using ApexCharts;
using AgenteSuporteGlpi.Web.Models;

namespace AgenteSuporteGlpi.Web.Services;

public class ConfiguracaoGraficosDashboard
{
    public ApexChartOptions<MGraficoCategoriaValor> CriarOpcoesBarrasPadrao()
        => new()
        {
            Theme = new Theme { Palette = PaletteType.Palette6 },
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false }
            },
            DataLabels = new DataLabels { Enabled = false },
            PlotOptions = new PlotOptions
            {
                Bar = new PlotOptionsBar
                {
                    BorderRadius = 10,
                    ColumnWidth = "48%"
                }
            },
            Grid = new Grid
            {
                BorderColor = "#E2E8F0"
            }
        };

    public ApexChartOptions<MGraficoCategoriaValor> CriarOpcoesAreaPadrao()
        => new()
        {
            Theme = new Theme { Palette = PaletteType.Palette2 },
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Zoom = new Zoom { Enabled = false }
            },
            DataLabels = new DataLabels { Enabled = false },
            Stroke = new Stroke
            {
                Curve = Curve.Smooth,
                Width = 3
            },
            Grid = new Grid
            {
                BorderColor = "#E2E8F0"
            }
        };
}
