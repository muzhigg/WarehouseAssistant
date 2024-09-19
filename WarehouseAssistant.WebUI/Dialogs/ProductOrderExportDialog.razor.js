export class ProductOrderExportDialog {
    static async downloadFileFromStream(fileName, contentStreamReference) {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);

        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName;
        anchorElement.click();

        URL.revokeObjectURL(url);
    }
}

window.ProductOrderExportDialog = ProductOrderExportDialog;
