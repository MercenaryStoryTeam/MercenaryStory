using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class CodeMergerEditor : EditorWindow
{
    // 병합할 폴더 경로와 결과 파일 경로
    string folderPath = "";
    string resultFile = "";
    string logMessage = "";

    Vector2 scrollPos;

    // Tools 메뉴에 "Code Merger" 항목 추가
    [MenuItem("Tools/Code Merger")]
    public static void ShowWindow()
    {
        GetWindow<CodeMergerEditor>("Code Merger");
    }

    // 에디터 창 GUI
    void OnGUI()
    {
        GUILayout.Label("코드 병합 설정", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("폴더 경로", folderPath);
        resultFile = EditorGUILayout.TextField("결과 파일 경로", resultFile);

        if (GUILayout.Button("코드 병합 실행"))
        {
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(resultFile))
            {
                logMessage = "폴더 경로와 결과 파일 경로를 입력하세요.";
            }
            else
            {
                try
                {
                    MergeCodes(folderPath, resultFile);
                    logMessage = "코드 병합이 완료되었습니다. 결과 파일: " + resultFile;
                }
                catch (Exception ex)
                {
                    logMessage = "코드 병합 중 오류 발생: " + ex.Message;
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("로그 메시지:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        EditorGUILayout.TextArea(logMessage, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 내용(content) 내부에 있는 연속된 backtick(`)의 최대 길이를 찾고,
    /// 외부에 사용할 fenced code block의 fence를 결정합니다.
    /// 만약 내용 중 backtick이 없으면 기본 3개를 사용하고,
    /// 있다면 그보다 1개 더 많은 backtick을 사용하여 중복을 피합니다.
    /// </summary>
    static string ChooseFence(string content)
    {
        var matches = Regex.Matches(content, "(`+)");
        int maxCount = 0;
        foreach (Match match in matches)
        {
            if (match.Value.Length > maxCount)
                maxCount = match.Value.Length;
        }
        int fenceCount = Math.Max(3, maxCount + 1);
        return new string('`', fenceCount);
    }

    /// <summary>
    /// Unity 내장 스크립트(예: Library, Packages, ProjectSettings 폴더 내의 파일) 여부를 판단합니다.
    /// </summary>
    static bool IsUnityBuiltInScript(string relPath)
    {
        // 유니티 내장 파일이 있는 폴더를 추가적으로 제외
        if (relPath.StartsWith("Library/") ||
            relPath.StartsWith("Packages/") ||
            relPath.StartsWith("ProjectSettings/"))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// baseFolder 내의 모든 파일을 재귀적으로 탐색하여,
    /// 각 파일의 상대 경로와 내용을 마크다운 형식(fenced code block)으로 resultFile에 저장합니다.
    /// Unity 내장 스크립트(내장 폴더의 파일)는 제외하며, 확장자가 .cs 인 사람 작성 스크립트만 포함합니다.
    /// </summary>
    static void MergeCodes(string baseFolder, string resultFile)
    {
        var mergedItems = new List<(string displayPath, string absPath)>();

        string baseFolderFull = Path.GetFullPath(baseFolder);

        // 확장자가 .cs 인 파일만 검색하도록 수정하여, 사람 작성 스크립트만 대상에 포함시킵니다.
        string[] files = Directory.GetFiles(baseFolderFull, "*.cs", SearchOption.AllDirectories);
        foreach (string absPath in files)
        {
            // baseFolder에 대한 상대 경로 추출 및 유닉스 스타일로 변환
            string relPath = absPath.Substring(baseFolderFull.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            relPath = relPath.Replace(Path.DirectorySeparatorChar, '/');

            // Unity 내장 스크립트는 병합 대상에서 제외
            if (IsUnityBuiltInScript(relPath))
                continue;

            // 출력 시 파일 경로 앞에 "./" 추가
            string displayPath = "./" + relPath;
            mergedItems.Add((displayPath, absPath));
        }

        // 결과 파일에 기록
        using (StreamWriter sw = new StreamWriter(resultFile, false, Encoding.UTF8))
        {
            // 경로순으로 정렬
            mergedItems.Sort((a, b) => string.Compare(a.displayPath, b.displayPath, StringComparison.Ordinal));
            foreach (var item in mergedItems)
            {
                sw.WriteLine($"{item.displayPath}:");
                string content = "";
                try
                {
                    content = File.ReadAllText(item.absPath, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    content = $"파일을 읽는 중 에러 발생: {ex.Message}";
                }
                string fence = ChooseFence(content);
                sw.WriteLine(fence);
                sw.Write(content);
                // 내용 마지막에 newline이 없다면 추가
                if (!content.EndsWith("\n"))
                {
                    sw.WriteLine();
                }
                sw.WriteLine(fence);
                sw.WriteLine();
            }
        }
    }
}
