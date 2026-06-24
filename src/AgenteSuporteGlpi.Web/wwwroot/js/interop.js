window.SenacUI = {
    toast: (icone, mensagem) => {
        if (!window.Swal) {
            console.warn('SweetAlert2 indisponivel. Toast nao exibido.', { icone, mensagem });
            return;
        }

        Swal.fire({
            icon: icone,
            title: mensagem,
            toast: true,
            position: 'top-end',
            timer: 3200,
            timerProgressBar: true,
            showConfirmButton: false
        });
    },

    confirmar: async (titulo, texto) => {
        if (!window.Swal) {
            return window.confirm(`${titulo}\n\n${texto}`);
        }

        const resultado = await Swal.fire({
            title: titulo,
            text: texto,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0057A8',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Confirmar',
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        });

        return resultado.isConfirmed;
    },

    downloadFile: (nomeArquivo, conteudoBase64) => {
        const link = document.createElement('a');
        link.download = nomeArquivo;
        link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + conteudoBase64;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};
