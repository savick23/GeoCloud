$ = (id) => document.getElementById(id);

function onKeyPress(e) {
//    let s = e.key;
//    /*switch (e.keyCode) {
//        case 
//    }*/
//    alert(e.keyCode);
//    $("cursor").before(s);
}
function onKeyDown(e) {
    switch (e.keyCode) {
        case 8:
            alert($('console').innerHTML);
            break;
        default:
            $("cursor").before(e.key);
            break;
    }
}
