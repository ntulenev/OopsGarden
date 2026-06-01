export async function fileToDataUrl(file, maxSide) {
    if (!file) return null;
    if (file.type.startsWith("image/")) {
        return resizeImageToDataUrl(file, maxSide);
    }

    return readFileAsDataUrl(file);
}

async function resizeImageToDataUrl(file, maxSide) {
    const image = await loadImage(file);
    const scale = Math.min(1, maxSide / Math.max(image.naturalWidth, image.naturalHeight));
    if (scale === 1) {
        return readFileAsDataUrl(file);
    }

    const canvas = document.createElement("canvas");
    canvas.width = Math.round(image.naturalWidth * scale);
    canvas.height = Math.round(image.naturalHeight * scale);
    const context = canvas.getContext("2d");
    context.drawImage(image, 0, 0, canvas.width, canvas.height);
    return canvas.toDataURL(file.type === "image/png" ? "image/png" : "image/jpeg", 0.86);
}

function loadImage(file) {
    return new Promise((resolve, reject) => {
        const image = new Image();
        image.onload = () => {
            URL.revokeObjectURL(image.src);
            resolve(image);
        };
        image.onerror = () => {
            URL.revokeObjectURL(image.src);
            reject(new Error("Image could not be loaded."));
        };
        image.src = URL.createObjectURL(file);
    });
}

function readFileAsDataUrl(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });
}
