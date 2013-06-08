// QueryWaitDlg.cpp : �����t�@�C��
//

#include "stdafx.h"
#include "EpgTimerTask.h"
#include "QueryWaitDlg.h"


// CQueryWaitDlg �_�C�A���O

CQueryWaitDlg::CQueryWaitDlg()
	: m_hDlg(NULL)
{
	m_bReboot = FALSE;
}

CQueryWaitDlg::~CQueryWaitDlg()
{
}

INT_PTR CQueryWaitDlg::DoModal()
{
	return DialogBoxParam(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD), NULL, DlgProc, (LPARAM)this);
}


// CQueryWaitDlg ���b�Z�[�W �n���h���[


BOOL CQueryWaitDlg::OnInitDialog()
{
	// TODO:  �����ɏ�������ǉ����Ă�������
	SetWindowLong(m_hDlg, GWL_EXSTYLE, GetWindowLong(m_hDlg, GWL_EXSTYLE) | WS_EX_DLGMODALFRAME);

	SetWindowPos(m_hDlg, HWND_TOPMOST, 0, 0, 0, 0, SWP_SHOWWINDOW|SWP_NOMOVE|SWP_NOSIZE);
	SetForegroundWindow(m_hDlg);
	if( m_bReboot == FALSE ){
		dwCount = 15;
		SetDlgItemText(m_hDlg, IDC_STATIC_MSG, L"�X�^���o�C�A�x�~�A�܂��̓V���b�g�_�E���ֈڍs���s���܂�");
	}else{
		SetDlgItemText(m_hDlg, IDC_STATIC_MSG, L"PC���ċN�����܂�");
		SetWindowText(m_hDlg, L"PC�ċN��");
		dwCount = 30;
	}
	SetTimer(m_hDlg, TIMER_WAIT_SLEEP, 1000, NULL );
	SendDlgItemMessage(m_hDlg, IDC_PROGRESS1, PBM_SETRANGE, 0, MAKELONG(0, dwCount));
	SendDlgItemMessage(m_hDlg, IDC_PROGRESS1, PBM_SETPOS, dwCount, 0);

	return TRUE;  // return TRUE unless you set the focus to a control
	// ��O : OCX �v���p�e�B �y�[�W�͕K�� FALSE ��Ԃ��܂��B
}


void CQueryWaitDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: �����Ƀ��b�Z�[�W �n���h���[ �R�[�h��ǉ����邩�A����̏������Ăяo���܂��B
	if( nIDEvent == TIMER_WAIT_SLEEP ){
		if( dwCount == 0 ){
			KillTimer(m_hDlg, TIMER_WAIT_SLEEP);
			EndDialog(m_hDlg, IDOK);
		}else{
			SendDlgItemMessage(m_hDlg, IDC_PROGRESS1, PBM_SETPOS, --dwCount, 0);
		}
	}
}


INT_PTR CALLBACK CQueryWaitDlg::DlgProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	CQueryWaitDlg* pSys = (CQueryWaitDlg*)GetWindowLongPtr(hDlg, GWLP_USERDATA);
	switch( uMsg ){
	case WM_INITDIALOG:
		SetWindowLongPtr(hDlg, GWLP_USERDATA, lParam);
		pSys = (CQueryWaitDlg*)lParam;
		pSys->m_hDlg = hDlg;
		return pSys->OnInitDialog();
	case WM_NCDESTROY:
		pSys->m_hDlg = NULL;
		break;
	case WM_TIMER:
		pSys->OnTimer(wParam);
		break;
	case WM_COMMAND:
		switch( LOWORD(wParam) ){
		case IDOK:
		case IDCANCEL:
			EndDialog(hDlg, LOWORD(wParam));
			return TRUE;
		}
		break;
	}
	return FALSE;
}
