$ = (id) => document.getElementById(id);
function onKeyDown(e) {
    switch (e.keyCode) {
        case 8: {
            let inp = $("input");
            let rng = document.createRange();
            rng.selectNodeContents(inp);
            let line = rng.toString();
            rng.setStart(inp, line.length - 2);
            rng.setEnd(inp, line.length - 1);
            rng.deleteContents();
            break;
        }
        case 37:
            break;
        case 32:
            $("cursor").before("\u00A0");
            break;
        case 13: input(); break;
        default:
            if (e.keyCode > 32 && e.keyCode <= 255)
                $("cursor").before(e.key);
            break;
    }
}
function input() {
    const inp = $("input");
    const line = inp.parentElement;
    let cmd = inp.innerText.trimRight();
    inp.remove();
    line.innerText += cmd;
    const ctrl = new AbortController();
    setTimeout(() => ctrl.abort(), 5000);
    try {
        fetch('console/input', {
            method: 'POST',
            processData: false,
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify({ input: cmd }),
            signal: ctrl.signal
        }).then(resp => resp.json()).then(lines => {
            lines.forEach(l => $("console").insertAdjacentHTML("beforeend", "<div>" + l + "</div>"));
            cursor();
        });
    }
    catch (e) {
        alert("Не удалось отправить команду! " + e);
    }
}
function cursor() {
    $("console").lastChild.insertAdjacentHTML("beforeend", "<div id=\"input\"><span id=\"cursor\">&nbsp;</span></div>");
}
