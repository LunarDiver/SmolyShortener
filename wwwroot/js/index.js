async function Shorten() {
    let text = document.getElementById("urlfield").value;

    let shortened;
    try {
        shortened = await $.post("/shorten", text);
    } catch (err) {
        alert(err.responseText);
        return;
    }

    let resel = document.getElementById("postresult");
    resel.innerHTML = shortened;
    resel.href = shortened;
    $('#postcollapse').collapse("show");
}