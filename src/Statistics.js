import styled from 'styled-components'

export default function Statistics({ elements_count }) {
  return (
    <StatisticsContainer>
      {Object.keys(elements_count).length > 0 &&
        Object.keys(elements_count).map((element, i) => {
          return (
            <ElementCountInfo key={element}>
              <ElementContainer>{element}</ElementContainer>
              :
              <ElementCountContainer>{elements_count[element]}</ElementCountContainer>
            </ElementCountInfo>
          );
        }
        )}
      {Object.keys(elements_count).length === 0 && (<div>No data</div>)}
    </StatisticsContainer>
  );
}

const StatisticsContainer = styled.div`
  height: 100%;
  min-width: 100px;

  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
`;

const ElementCountInfo = styled.div`
  display: flex;
  flex-direction: row;

  text-align: center;
`;

const ElementContainer = styled.div``;
const ElementCountContainer = styled.div``;
