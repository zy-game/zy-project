import('./help.js')
window.url = "http:127.0.0.1:5001/api/"
layui.use('element', function () {
    window.$ = layui.jquery, element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
    element.on('tab(__tab)', function (elem) {
        OnSwitchPages(elem.index)
    });
    window.$(document).on('click', "#btn_search", function () {
        onSearchText(document.getElementById("search_text").value)
    });
    window.$(document).on('click', "#btn_chat_gpt", function () {
        GetChatGPTResponse(document.getElementById("chat_gpt_input").value)
    });
})
function OnSwitchPages(index) {
    switch (index) {
        case 0:
            ShowHomePage()
            break
        case 1:
            ShowHomePage()
            break
        case 1:
            ShowHomePage()
            break
        case 3:
            ShowChatGPTPage()
            break
        case 4:
            ShowChatGPTPage()
            break
    }
}

function onSearchText(str) {
    console.log(str)
}
function ShowUnityPage() {
    document.getElementById("chat").style.display = "none"
}

function ShowChatGPTPage() {
    document.getElementById("chat").style.display = "block"
}

function GetChatGPTResponse(input) {
    if (String(input).length <= 0) {
        window.alert("先输入提示语，在开始玩耍！")
        return
    }
    get("chatgpt/" + input, function (args) {
    })
}

function ShowHomePage() {
    document.getElementById("chat").style.display = "none"
    get("web", function (args) {
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
            $(document).on("click", "#test_" + temp_id, function () {
                get("web/" + id, function (args) {
                })
            })
        }
    })
}
