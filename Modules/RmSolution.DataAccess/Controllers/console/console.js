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
        case 16:
        case 17:
        case 18:
            break;
        default:
            input(e.key);
            break;
    }
}
function input(symb) {
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
    $("console").insertAdjacentHTML("beforeend", "<span id=\"cursor\">&nbsp;</span>");
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
                        if (value.length > 1 || value[0] !== 0) {
                            if (value[0] === 8) {
                                let rng = document.createRange();
                                rng.setEndBefore($("cursor"));
                                rng.setStart($("console"), rng.endOffset - 1);
                                rng.deleteContents();
                            }
                            else $("cursor").insertAdjacentHTML("beforebegin", utf8String(value));
                        }
                        push();
                    });
                }
                push();
            },
        });
    });
}
