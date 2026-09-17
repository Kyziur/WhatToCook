window.whatToCook = window.whatToCook || {};

window.whatToCook.scrollToElement = (elementId) => {
    document.getElementById(elementId)?.scrollIntoView({ behavior: "smooth", block: "start" });
};

window.whatToCook.createObjectUrl = (fileInput) => {
    const [file] = fileInput.files;
    return file ? URL.createObjectURL(file) : null;
};

window.whatToCook.revokeObjectUrl = (url) => URL.revokeObjectURL(url);
