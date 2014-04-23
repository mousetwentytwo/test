/// <reference path="f3.plugin.common.js" />

function scrollOnResize()
{
    if ($('.c_selected').size() == 1) $.scrollTo($('.c_selected'), 200, { axis: 'x', margin: true });
}

function selectOnScroll()
{
    $('div.f3.content').each(function()
    {
        if ($(window).scrollLeft() == $(this).position().left)
        {
            btn_click(this.id.replace("cnt_", "btn_"));
        }
    });
}

function scrollOnLoad()
{
    setTimeout(function()
    {
        if ($('.c_selected').size() == 1) $.scrollTo({ left: $('.c_selected').position().left, top: 0 }, 200, { axis: 'yx', margin: true });
    }, 300);
}

function onResize()
{
    scrollOnResize();
    //console.log($('[data-resize]'));
}

var timeOutResize = false;
$(window).resize(function () {
    if (timeOutResize !== false)
        clearTimeout(timeOutResize);
    timeOutResize = setTimeout(onResize, 200);
});



function btn_click(btn, top)
{
	var oBtn;
	if (typeof(btn) == 'string')
	{
		oBtn = document.getElementById(btn);
	}
	else
	{
		oBtn = btn;
	}
	
	var oMenu = document.getElementById('menu');
	var oButtons = oMenu.getElementsByTagName('div');
	var sContentId = '';
	for (var i = 0; i < oButtons.length; i++)
	{
		sContentId = oButtons[i].id.replace("btn_", "cnt_");
		if (oButtons[i].id == oBtn.id)
		{
			oButtons[i].className = oButtons[i].className.replace("mb_notselected", "mb_selected");
			document.getElementById(sContentId).className = document.getElementById(sContentId).className.replace("c_notselected", "c_selected");
			$.scrollTo({ left: $('#' + sContentId).position().left, top: (typeof top == 'undefined') ? 0 : top }, 500, { axis: 'yx', margin: true });
		}
		else
		{
			oButtons[i].className = oButtons[i].className.replace("mb_selected", "mb_notselected");
			document.getElementById(sContentId).className = document.getElementById(sContentId).className.replace("c_selected", "c_notselected");
		}
		
	}
}

function frm_contact_validate(fld)
{
    var oFld;
    if (typeof (fld) == 'string') {
        oFld = document.getElementById(fld);
    }
    else {
        oFld = fld;
    }

    var ok = true;
    var oFields;
    if (oFld === null || fld === undefined)
    {
        oFields = document.getElementsByClassName('text');
    }
    else
    {
        oFields = [fld];
    }
	for (i = 0; i < oFields.length; i++)
	{
		if (oFields[i].value === "")
		{
			ok = false;
			if (oFields[i].parentElement.className.search('required') == -1)
			{
				oFields[i].parentElement.className = oFields[i].parentElement.className.concat(' required');
			}
		}
		else
		{
			oFields[i].parentElement.className = oFields[i].parentElement.className.replace(' required', '');
		}
    }

    return ok;
}

function btn_send_click()
{
    if (frm_contact_validate() !== true)
    {
        $('#required').fadeIn('fast');
        setTimeout(function () { $('#required').fadeOut('slow'); }, 1500);
    }
    else
    {
        alert('todo');
    }
}