$ = (id) => document.getElementById(id);

function onKeyPress(e) {
    let s = e.key;
    $("cursor").before(s);
}