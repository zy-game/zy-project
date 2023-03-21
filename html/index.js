import('./help.js')
window.url = "http:127.0.0.1:5001/api/"
layui.use('element', function () {
    window.$ = layui.jquery, element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
    element.on('tab(__tab)', OnSwitchView);
    window.$(document).on('click', "#btn_search", onSearchText);
    window.$(document).on('click', "#btn_chat_gpt", OnGetChatGPTData);
})
function OnSwitchView(elem) {
    switch (elem.index) {
        case 0:
            ShowHomeView()
            break
        case 1:
            ShowUnityView()
            break
        case 2:
            ShowCsharpView()
            break
        case 3:
            ShowChatGPTView()
            break
        case 4:
            ShowOhterView()
            break
    }
}
function OnGetChatGPTData() {
    let input = document.getElementById("chat_gpt_input").value
    if (String(input).length <= 0) {
        window.alert("先输入提示语，在开始玩耍！")
        return
    }
    request("chatgpt/code" + input, function (args) {
    })
}
function ShowCsharpView() {
    document.getElementById("chat").style.display = "none"
}

function ShowOhterView() {
    document.getElementById("chat").style.display = "none"
}

function onSearchText() {
    let input = document.getElementById("search_text").value
    document.getElementById("search_text").value = ""
    if (String(input).length <= 0) {
        return
    }
    request("search/" + input, function (args) {
    })
}

function ShowUnityView() {
    document.getElementById("chat").style.display = "none"
}

function ShowChatGPTView() {
    document.getElementById("chat").style.display = "block"
}

function ShowHomeView() {
    document.getElementById("chat").style.display = "none"
    request("web", function (args) {
        var list = document.getElementById("list")
        // for (i = 0; i < list.childElementCount; i++) {
        //     list.removeChild(list.childNodes[i])
        // }
        let jsonList = JSON.parse(data)
        var result = "";
        for (i = 0; i < jsonList.length; i++) {
            result += getTimelineItemString(jsonList[i])
        }
        list.innerHTML = result
        for (i = 0; i < jsonList.length; i++) {
            let temp_id = jsonList[i].id
            $(document).on("click", "#" + temp_id, function () {
                request("web/" + id, function (args) {
                })
            })
        }
    })
}
