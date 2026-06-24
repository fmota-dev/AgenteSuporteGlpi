using ClosedXML.Excel;

namespace AgenteSuporteGlpi.Web.Services;

public class ServicoExportacao
{
    public byte[] GerarExcel<T>(IEnumerable<T> dados, string nomePlanilha = "Relatorio")
    {
        using var pastaTrabalho = new XLWorkbook();
        var planilha = pastaTrabalho.Worksheets.Add(nomePlanilha);
        var propriedades = typeof(T).GetProperties();
        var lista = dados.ToList();

        for (var indiceColuna = 0; indiceColuna < propriedades.Length; indiceColuna++)
        {
            var celula = planilha.Cell(1, indiceColuna + 1);
            celula.Value = propriedades[indiceColuna].Name;
            celula.Style.Font.Bold = true;
            celula.Style.Fill.BackgroundColor = XLColor.FromHtml("#0057A8");
            celula.Style.Font.FontColor = XLColor.White;
        }

        for (var indiceLinha = 0; indiceLinha < lista.Count; indiceLinha++)
        {
            for (var indiceColuna = 0; indiceColuna < propriedades.Length; indiceColuna++)
            {
                var valor = propriedades[indiceColuna].GetValue(lista[indiceLinha]);
                planilha.Cell(indiceLinha + 2, indiceColuna + 1).Value = valor?.ToString() ?? string.Empty;
            }

            if (indiceLinha % 2 == 1)
            {
                planilha.Row(indiceLinha + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FB");
            }
        }

        planilha.Columns().AdjustToContents();
        planilha.SheetView.FreezeRows(1);

        using var fluxo = new MemoryStream();
        pastaTrabalho.SaveAs(fluxo);
        return fluxo.ToArray();
    }
}
