<script setup>
import DxStore from "@/utils/dx";
import { useCursoStore } from "@/stores";
import { ref, onMounted, toRaw, getCurrentInstance } from "vue";
import { useRouter, useRoute } from "vue-router";
import {
  DxColumn,
  DxColumnChooser,
  DxColumnFixing,
  DxDataGrid,
  DxEditing,
  DxExport,
  DxFilterRow,
  DxGrouping,
  DxGroupItem,
  DxGroupPanel,
  DxLoadPanel,
  DxLookup,
  DxPager,
  DxPaging,
  DxScrolling,
  DxSearchPanel,
  DxSorting,
  DxSummary,
} from "devextreme-vue/data-grid";

const app = getCurrentInstance(), router = useRouter(), route = useRoute(), storeCursos = useCursoStore();
let periods = [], width = 120, data = [], cursoId = ref(null), dxStore = ref(null),
  curso = ref(null), fechas = ref([]), asistencias = ref([]), grid = null, props = app.appContext.config.globalProperties;

let cancel = () => {
  console.clear();
  // console.log(_sep);
  storeCursos.item = data;
  // console.log("Cancel in tabs!");
  router.push("/admin/cursos");
};

let save = () => {
  $("#data").lock("Guardando asistencia");
  let rows = grid.getVisibleRows();
  rows.forEach(row => {
    var participante = row.data;
    console.log("participante =>", participante);
  });
  setTimeout(function () {
    $("#data").unlock();
    msg.success("¡Asistencia guardada!", `<span class="d-inline-block mt-2">La asistencia para el curso "<span class="font-weight-semibold mt-3">${curso.value.nombre}"</span>, fue guardada de manera exitosa.</span>`, function () {
      // router.back();
    });
  }, 1000);
}

let getData = () => {
  dxStore.value = DxStore({
    key: ["id"],
    endPoint: "curso/participantes-dx",
    userData: JSON.stringify({ id: cursoId.value }),
    onLoading: function (loadOptions) {
      $("#grid").lock("Cargando");
      // console.log("loadOptions =>", loadOptions);
      // console.log("onLoading");
    },
    onLoaded: function (results) {
      // console.log("results =>", results);
      // console.log("onLoaded!");
      data = toRaw(results.data);
      // console.log("data loaded =>", data);
      $("#grid").unlock();
      $("#data").unlock();
    },
  });
};

let customizeColumns = (cols) => {
  let idx = cols.length - 1;
  let col = cols[idx];
  if (col.isBand) {
    // console.log("col =>", col.caption);
    var counter = periods.length;
    periods.forEach(period => {
      cols[idx - counter].ownerBand = idx;
      counter--;
    });
  } else {
    // console.log("col =>", col);
  }
  console.log("customizeColumns");
};

let onInitialized = (e) => {
  // console.clear();
  grid = e.component;
  // console.log("grid =>", grid);
  var colWidth = width / periods.length;
  // console.log("fechas.value =>", toRaw(fechas.value));
  fechas.value.forEach(item => {
    // console.log("fecha =>", item);
    periods.forEach(period => {
      grid.addColumn({
        width: colWidth,
        caption: period,
        alignment: "center",
        encodeHtml: false,
        customizeText: function (data) {
          // console.log("data =>", data);
          return `<input type="checkbox" id="-f${item.id}-${period.split()[0].toLowerCase()}" class="thumb" />`;
        },
      });
    });
    grid.addColumn({
      isBand: true,
      alignment: "center",
      caption: props.$formatDate(item.fechaClase)
    });
  });
  grid.addColumn({
    width: 90,
    caption: "Porcentaje",
    alignment: "center",
    encodeHtml: false,
    customizeText: function (data) {
      // console.log("data =>", data);
      return `<span id="m1-">&nbsp;</span>`;
    },
  });
  grid.addColumn({
    width: 75,
    caption: "Aprueba",
    alignment: "center",
    encodeHtml: false,
    customizeText: function (data) {
      // console.log("data =>", data);
      return `<span id="m2-">&nbsp;</span>`;
    },
  });
  console.log("onInitialized");
};

let calcular = async (el) => {
  console.clear();
  let data = el.data();
  let checked = el.is(":checked");
  data.asistencia.asistio = checked;
  let pId = data.participante.id;
  let els = $(`input[id*="${pId}"]`);
  console.log("data.fecha.ponderacion =>", data.fecha.ponderacion);
  let prom = 0;
  els.each(function (index) {
    var el = $(this);
    if (el.is(":checked")) prom += el.data().fecha.ponderacion / periods.length;
  });
  let p = parseInt(prom);
  console.log("prom =>", prom);
  data.participante.porcentaje = p;
  $("span#p-" + pId).data("val", p).html(p + "%");
  let a = prom < curso.value.porcentajeValidoAsistencia;
  data.participante.aprueba = !a;
  $("span#a-" + pId).data("val", a).html(a ? "NO" : "SI");
  console.log("el =>", el);
  console.log("data =>", data);
  console.log("checked =>", checked);
  let item = await storeCursos.asistenciaGuardar({ cursoUsuario: data.participante, asistencia: data.asistencia });
}

let onReady = (e) => {
  // console.log("onReady =>", e);
  let rows = grid.getVisibleRows();
  rows.forEach(row => {
    var participante = row.data;
    // console.log("data =>", row.data);
    row.cells.forEach(cell => {
      if (cell.text.contains("checkbox")) {
        console.log(_sep);
        console.log("participante =>", participante);
        let chk = $(cell.cellElement).find("input");
        console.log("el =>", chk);
        let id = chk.attr("id");
        console.log("id =>", id);
        let segments = id.replace("-f", "").split("-");
        let fechaId = parseInt(segments[0]);
        console.log("fechaId =>", fechaId);
        let fechaObj = toRaw(fechas.value.find(o => o.id == fechaId));
        let jornada = segments[1];
        console.log("jornada =>", jornada);
        let asistencia = toRaw(asistencias.value.find(o => o.usuarioId == participante.usuarioId
          && o.jornada == jornada && o.fecha == fechaObj.fechaClase));
        console.log("asistencia =>", asistencia);
        chk.attr("checked", asistencia.asistio);
        console.log("f =>", fechaObj);
        id = "p" + participante.id + id;
        chk.data("id", id)
        chk.data("fecha", fechaObj);
        chk.data("participante", row.data);
        chk.data("asistencia", asistencia);
        chk.attr("id", id);
        chk.click(function (ev) {
          let el = $(this);
          calcular(el);
        });
      }
      if (cell.text.contains("m1-")) { // Porcentaje
        let el = $(cell.cellElement);
        el.html(`<span id="p-${participante.id}">${participante.porcentaje}%</span>`);
      }
      if (cell.text.contains("m2-")) { // Aprueba
        let el = $(cell.cellElement);
        el.html(`<span id="a-${participante.id}">${participante.aprueba ? "SI" : "NO"}%</span>`);
        // el.html(`<span id="a-${row.data.id}">NO</span>`);
      }
    });
  });
  // let rowIndex = grid.getRowIndexByKey(e.key);
  // let row = rows[rowIndex];
  // // console.log(row.data);
}

function importar(params) {

}

defineExpose({ cancel });

onMounted(async () => {
  console.clear();
  console.log(_sep);
  console.log("asistecnia.vue mounted!");
  $("#data").lock("Cargando asistencia");
  const id = route.params.id;
  // console.log("id =>", id);
  // console.log("id != null", id != null);
  // console.log("id.length", id.length);
  if (id.length > 0) {
    cursoId.value = 1;
    fechas.value = await storeCursos.fechas(id);
    asistencias.value = await storeCursos.asistencias(id);
    if (storeCursos.item != null && storeCursos.item.id == parseInt(id)) curso.value = storeCursos.item;
    else curso.value = await storeCursos.porId(id);
    if (curso.value.jornadaManana) periods.push(fechas.value.length > 2 ? 'M' : 'Mañana');
    if (curso.value.jornadaTarde) periods.push(fechas.value.length > 2 ? 'T' : 'Tarde');
    if (curso.value.jornadaNoche) periods.push(fechas.value.length > 2 ? 'N' : 'Noche');
    // width = periods.length == 3 ? 120 : periods.length == 2 ? 80 : periods.length == 1 ? 40 : 40;
    console.log("curso =>", toRaw(curso.value));
    // console.log("CARGAR CURSO!!");
    cursoId.value = parseInt(id);
  } else {
    cursoId.value = 0;
  }
  getData();
});
//----------------------------------------------------------------------------------------------------------------------------------------------
</script>
<template>
  <div class="container content">
    <div class="card data mh" id="data">
      <div class="card-header main d-flex justify-content-between align-items-end">
        <span class="d-flex justify-content-between">
          <i class="fa-solid fa-gears"></i>
          <span v-if="curso">Administración &raquo; Asistencia<br />"{{ curso.nombre }}"</span>
        </span>
        <span>
          <router-link :to="{ path: '/admin/inscritos' }" class="btn btn-trans" title="Volver">
            <i class="fa-solid fa-circle-arrow-left me-1"></i>VOLVER
          </router-link>
          <a href="#" @click.prevent="importar()" class="btn btn-trans ms-4" title="Importar asistencia">
            <i class="fa-solid fa-cloud-arrow-up me-1"></i> IMPORTAR
          </a>
          <a href="#" @click.prevent="save()" class="btn btn-trans ms-4" title="Guardar asistencia">
            <i class="fa-solid fa-floppy-disk me-1"></i> GUARDAR
          </a>
          <!-- <a href="#" @click.prevent="storeCursos.codigos()" class="btn btn-trans ms-4">
            ACTUALIZAR CÓDIGOS <i class="ms-1 fa-solid fa-circle-arrow-right"></i>
          </a> -->
        </span>
      </div>
      <div class="card-body pt-4" v-if="curso">
        <div class="row">
          <div class="col-md-12 mb-2 d-flex justify-content-between">
            <p class="mb-3">
              <i class="fa-solid fa-circle-info me-1 color-main"></i> Lorem
              ipsum dolor sit amet, consectetur adipiscing elit.
            </p>
            <p class="mb-3"><b>Participantes en el curso:</b> {{ curso.participantes }}</p>
          </div>
        </div>
        <div class="row">
          <div class="col-md-12">
            <DxDataGrid :column-auto-width="false" :customize-columns="customizeColumns" :data-source="dxStore"
              :hover-state-enabled="true" :remote-operations="false" :repaint-changes-only="true"
              :row-alternation-enabled="true" :show-borders="false" :word-wrap-enabled="true"
              horizontal-alignment="Stretch" @initialized="onInitialized" @content-ready="onReady" id="gridContainer"
              key-expr="id" css-class="diego">
              <DxColumnChooser :enabled="false" mode="dragAndDrop" />
              <DxColumnFixing :enabled="true" />
              <DxEditing :allow-updating="false" :allow-deleting="false" :allow-adding="false" mode="cell" />
              <DxExport :enabled="false" />
              <DxFilterRow :visible="false" />
              <DxGrouping :auto-expand-all="true" />
              <DxGroupPanel :visible="false" :allow-column-dragging="true" />
              <DxLoadPanel :enabled="false" />
              <DxScrolling row-rendering-mode="virtual" />
              <DxSearchPanel :visible="false" :highlight-case-sensitive="false" />
              <DxSorting mode="none" />
              <DxSummary>
                <DxGroupItem summary-type="count" column="group_type_name" display-format="{0} ítems" />
              </DxSummary>
              <DxPaging :page-size="10000" />
              <DxPager :visible="false" :show-info="true" :show-page-size-selector="false" :show-navigation-buttons="true"
                :allowed-page-sizes="[15, 50, 'Todos']" info-text="{2} participantes (página {0} de {1})" />
              <DxColumn data-field="name" caption="Nombre" :visible="true" :fixed="false" fixed-position="left" />
              <DxColumn data-field="email" caption="Correo" :visible="true" :fixed="false" fixed-position="left" />
            </DxDataGrid>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
