SELECT * FROM Process
SELECT * FROM Schedules

-- 스케줄이 부모, 프로세스가 자식(SchIdx로 연결되어있음)
SELECT s.* FROM Schedules AS s
INNER JOIN Process AS p
ON s.SchIdx = p.SchIdx

-- 1. Prc Result에서 성공개수와 실패개수를 다른 (가상)컬럼으로 분리
SELECT p.SchIdx, p.PrcDate, 
       CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,    -- WHEN 1은 성공, THEN 1은 그냥 숫자 1
	   CASE p.PrcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail   -- WHEN 0은 실패, THEN 1은 그냥 숫자 1
  FROM Process AS p


-- 2. 합계 집계(서브 쿼리 사용)
SELECT smr.SchIdx, smr.PrcDate, 
       SUM(smr.PrcOK) AS PrcOkAmount, SUM(smr.PrcFail) AS PrcFailAmount
  FROM ( 
        SELECT p.SchIdx, p.PrcDate, 
               CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,    
	           CASE p.PrcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail   
          FROM Process AS p
       ) AS smr
GROUP BY smr.SchIdx, smr.PrcDate


-- 3.0 조인문
SELECT * FROM Schedules AS sch
INNER JOIN Process AS prc
ON sch.SchIdx = Prc.SchIdx

-- 3.1 2번결과(가상테이블)과 Schedules 테이블을 조인해서 원하는 결과 도출
 SELECT sch.SchIdx, sch.PlantCode, sch.SchAmount, prc.PrcDate,
        prc.PrcOkAmount, prc.PrcFailAmount
   FROM Schedules AS sch
INNER JOIN (
			SELECT smr.SchIdx, smr.PrcDate, 
				   SUM(smr.PrcOK) AS PrcOkAmount, SUM(smr.PrcFail) AS PrcFailAmount
			  FROM ( 
					 SELECT p.SchIdx, p.PrcDate, 
					  	   CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,    
						   CASE p.PrcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail   
					  FROM Process AS p
				   )  AS smr
GROUP BY smr.SchIdx, smr.PrcDate
) AS prc
  ON sch.SchIdx = Prc.SchIdx


