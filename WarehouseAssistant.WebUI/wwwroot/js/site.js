function DownloadExcelFile(filename, content) {
    // Create the URL
    const file = new File([content], filename, {type: "application/octet-stream"});
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = "_self";
    a.click();

    URL.revokeObjectURL(exportUrl);
}

function playNotificationSound(url) {
    let audio = new Audio(url);
    audio.play();
}