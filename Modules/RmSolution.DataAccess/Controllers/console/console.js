$ = (id) => document.getElementById(id);
const utf8String = (bytes) => {
    var result = '';
    for (var i = 0; i < bytes.length; ++i) {
        const byte = bytes[i];
        const text = byte.toString(16);
        result += (byte < 16 ? '%0' : '%') + text;
    }
    return decodeURIComponent(result);
};
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
     //   case 13: input(); break;
        default:
            if (e.keyCode > 32 && e.keyCode <= 255)
                input(e.key);
                //$("cursor").before(e.key);
            break;
    }
}
function input(symb) {
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
            body: JSON.stringify({ input: symb }),
            signal: ctrl.signal
        });
    }
    catch (e) {
        alert("Не удалось отправить строку! " + e);
    }
}
function cursor() {
    $("console").lastChild.insertAdjacentHTML("beforeend", "<div id=\"input\"><span id=\"cursor\">&nbsp;</span></div>");
}

function start() {
    fetch("console/read").then((resp) => {
        const rd = resp.body.getReader();
        return new ReadableStream({
            start(controller) {
                function push() {
                    return rd.read().then(({ done, value }) => {
                        if (done) {
                            controller.close();
                            return;
                        }
                        $("console").insertAdjacentHTML("beforeend", utf8String(value)), 
                        push();
                    });
                }
                push();
            },
        });
    });
}
