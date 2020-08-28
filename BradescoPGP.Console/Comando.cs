namespace BradescoPGP.Console
{
    public enum Comando
    {
        CopiarArquivosParaDownload,
        ImportarAniversarios,
        ImportarInvestFacil,
        ImportarQualitativo,
        ImportarVencimentos,
        ImportarCorretora,
        ImportarCockpit,
        ImportarTemInvestFacil,
        ImportarAplicacaoResgate,
        Expurgo,
        ImportarPortabilidade,
        ImportarClusHieEnc,
        ImportarTopTier,
        ImportarClusTopTier,
        ImportarCaptacaoLiquida    //Este processo chama internamente o processo ImportarCaminhoDinheiro, não sendo necessario declara-lo aqui
        //ImportarHierarquias,
        //ImportarEncarteiramento,
        //ImportarClusterizacao,
    }
}
