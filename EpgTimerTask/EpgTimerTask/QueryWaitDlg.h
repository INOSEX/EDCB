#pragma once

#define TIMER_WAIT_SLEEP 100
// CQueryWaitDlg �_�C�A���O

class CQueryWaitDlg
{
public:
	CQueryWaitDlg();   // �W���R���X�g���N�^�[
	~CQueryWaitDlg();
	INT_PTR DoModal();

	void SetRebootMode(){ m_bReboot = TRUE; };

// �_�C�A���O �f�[�^
	enum { IDD = IDD_DIALOG_QUERY_WAIT };

protected:
	HWND m_hDlg;
	DWORD dwCount;
	BOOL m_bReboot;

	BOOL OnInitDialog();
	void OnTimer(UINT_PTR nIDEvent);
	static INT_PTR CALLBACK DlgProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
};
